using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VisualHFT.Commons.Helpers;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Commons.Pools;
using VisualHFT.Enums;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.MarketResilience.Model;
using VisualHFT.Studies.MarketResilience.UserControls;
using VisualHFT.Studies.MarketResilience.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    internal class DepletionState
    {
        private OrderBook _iniOrderBook;
        private OrderBook _endOrderBook;
        private eLOBSIDE _depletionSide;
        private eLOBSIDE _recoveredSide;
        private double? _depletionInitialPrice;
        private DateTime? _initialTimestamp;
        private DateTime? _endTimestamp;
        private TimeSpan _timeout;

        public DepletionState(TimeSpan timeout)
        {
            _timeout = timeout;
        }
        public eLOBSIDE DepletionSide { get => _depletionSide; }

        public TimeSpan Elapsed
        {
            get
            {
                if (_initialTimestamp == null || _endTimestamp == null) 
                    return TimeSpan.Zero;
                return _endTimestamp.Value - _initialTimestamp.Value;
            }
        }
        public TimeSpan PartialElapsed
        {
            get
            {
                if (_initialTimestamp == null || _endTimestamp != null)
                    return TimeSpan.Zero;
                return HelperTimeProvider.Now - _initialTimestamp.Value;
            }
        }
        public void SetIniOrderBook(OrderBook e, eLOBSIDE depletionSide, double initialDepletionPrice)
        {
            if (_iniOrderBook == null)
                _iniOrderBook = new OrderBook(e.Symbol, e.PriceDecimalPlaces, e.MaxDepth);
            
            _iniOrderBook.ShallowCopyFrom(e, null);
            _endOrderBook = null;
            _depletionSide = depletionSide;
            _initialTimestamp = HelperTimeProvider.Now;
            _endTimestamp = null;
            _depletionInitialPrice = initialDepletionPrice;
        }
        public void SetEndOrderBook(OrderBook e, eLOBSIDE recoveredSide)
        {
            if (_endOrderBook == null)
                _endOrderBook = new OrderBook(e.Symbol, e.PriceDecimalPlaces, e.MaxDepth);
            _endOrderBook.ShallowCopyFrom(e, null);
            _recoveredSide = recoveredSide;
            _endTimestamp = HelperTimeProvider.Now;
        }



        public eLOBSIDE GetBookRecoverySide(OrderBook currentOrderBook)
        {
            double bestBid = currentOrderBook.GetTOB(true).Price.Value;
            double bestAsk = currentOrderBook.GetTOB(false).Price.Value;

            if (_depletionSide == eLOBSIDE.BID)
            {
                // Check if bids have recovered
                if (bestBid >= _depletionInitialPrice)
                {
                    return eLOBSIDE.BID; // Bids have recovered
                }
                else
                {
                    return eLOBSIDE.ASK; // Asks have taken over
                }
            }
            else if (_depletionSide == eLOBSIDE.ASK)
            {
                // Check if asks have recovered
                if (bestAsk <= _depletionInitialPrice)
                {
                    return eLOBSIDE.ASK; // Asks have recovered
                }
                else
                {
                    return eLOBSIDE.BID; // Bids have taken over
                }
            }

            return eLOBSIDE.NONE;
        }

        public bool IsTimeout()
        {
            return PartialElapsed > _timeout;
        }
        public bool IsRunning()
        {
            return (_initialTimestamp.HasValue && !_endTimestamp.HasValue);
        }
        public void Reset()
        {
            _iniOrderBook = null;
            _endOrderBook = null;
            _depletionSide = eLOBSIDE.NONE;
            _initialTimestamp = null;
            _endTimestamp = null;
            _depletionInitialPrice = null;
        }
    }
    public class MarketResilienceStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;




        private OrderBook _previousOrderBook;
        private DepletionState _depletionStateHolder;
        
        private List<double> _recentSpreads;

        private const int recentSpreadWindowSize = 50;
        private int LEVELS_TO_CONSUME = 3;
        private double SPREAD_INCREASED_BY = 1.0; //by 100%
        private TimeSpan _depletionRecoveryTimeOut = TimeSpan.FromMilliseconds(500);

        private CustomObjectPool<BookItem> _poolBookItems;

        private HelperCustomQueue<OrderBook> _QUEUE;

        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;


        public override string Name { get; set; } = "Market Resilience Study";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Market Resilience";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "MR";
        public override string TileToolTip { get; set; } =
            "<b>Market Resilience</b> (MR) is a real-time metric that quantifies how quickly a market rebounds after experiencing a large trade.<br/>" +
            "It's an invaluable tool for traders to gauge market stability and sentiment.<br/><br/>" +
            "The <b>MktRes</b> score is a composite index derived from three key market behaviors:<br/>" +
            "1. <b>Time Recovery:</b> Measures the elapsed time since the large trade, normalized between 0 (immediate recovery) and 1 (close to timeout).<br/>" +
            "2. <b>Spread Recovery:</b> Measures how quickly the gap between buying and selling prices returns to its normal state after a large trade.<br/>" +
            "3. <b>Depth Recovery:</b> Assesses how fast the consumed levels of the Limit Order Book (LOB) are replenished post-trade.<br/><br/>" +
            "The <b>MktRes</b> score is calculated by taking a weighted average of these three normalized metrics, ranging from 0 (no recovery) to 1 (full recovery). The score is then adjusted based on whether the market has recovered on the same side as the depletion.<br/><br/>" +
            "<b>Resilience Strength</b><br/>" +
            "1. <b>Strong Resilience:</b> Resilience Score ≥ 0.7<br/>" +
            "2. <b>Moderate Resilience:</b> 0.3 ≤ Resilience Score &lt; 0.7<br/>" +
            "3. <b>Weak Resilience:</b> Resilience Score &lt; 0.3<br/><br/>" +
            "<b>Market Resilience Bias</b><br/>" +
            "- If the market has a strong resilience and recovers on the same side as the depletion, it indicates a bias in the opposite direction of the recovery.<br/>" +
            "- If the market has a weak resilience and recovers on the opposite side of the depletion, it indicates a bias in the same direction as the recovery.<br/><br/>" +
            "<b>Quick Guidance for Traders:</b><br/>" +
            "- Use the <b>MR</b> score to gauge the market's reaction to large trades.<br/>" +
            "- A high score indicates a robust market that recovers quickly, ideal for entering or exiting positions.<br/>" +
            "- A low score suggests the market is more vulnerable to large trades, so exercise caution.<br/>" +
            "- Adjust your trading strategy based on the resilience strength: strong, moderate, or weak.<br/>" +
            "- Pay attention to the Market Resilience Bias to understand market sentiment and potential directional moves.";

        public MarketResilienceStudy()
        {
            _recentSpreads = new List<double>();
            _depletionStateHolder = new DepletionState(_depletionRecoveryTimeOut);

            _previousOrderBook = new OrderBook();

            _QUEUE = new HelperCustomQueue<OrderBook>($"<OrderBook>_{this.Name}", QUEUE_onRead, QUEUE_onError);
        }
        ~MarketResilienceStudy()
        {
            Dispose(false);
        }

        public override async Task StartAsync()
        {
            await base.StartAsync();//call the base first
            
            _poolBookItems = new CustomObjectPool<BookItem>(3000);
            _recentSpreads.Clear();
            _depletionStateHolder.Reset();
            _previousOrderBook.Reset();

            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);

            log.Info($"{this.Name} Plugin has successfully started.");
            Status = ePluginStatus.STARTED;
        }

        public override async Task StopAsync()
        {
            Status = ePluginStatus.STOPPING;
            log.Info($"{this.Name} is stopping.");

            _recentSpreads.Clear();
            _depletionStateHolder.Reset();
            _previousOrderBook.Reset();
            HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
            _QUEUE.Clear();
            _poolBookItems.Dispose();
            _poolBookItems = null;

            await base.StopAsync();
        }

        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            /*
             * ***************************************************************************************************
             * TRANSFORM the incoming object (decouple it)
             * DO NOT hold this call back, since other components depends on the speed of this specific call back.
             * DO NOT BLOCK
               * IDEALLY, USE QUEUES TO DECOUPLE
             * ***************************************************************************************************
             */

            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;

            var currentLOB = new OrderBook();
            currentLOB.ShallowCopyFrom(e, _poolBookItems);
            _QUEUE.Add(currentLOB);
        }
        private void QUEUE_onRead(OrderBook e)
        {
            if (e == null) return;

            if (_depletionStateHolder.IsRunning())
            {
                if (IsSpreadBackToItsAvg(e))
                {
                    var recoverySide = _depletionStateHolder.GetBookRecoverySide(e);
                    if (recoverySide != eLOBSIDE.NONE)
                    {
                        _depletionStateHolder.SetEndOrderBook(e, recoverySide);
                        TimeSpan timeSinceDepletion = _depletionStateHolder.Elapsed;
                            
                        var resilienceScore = CalculateMarketResilienceScore(timeSinceDepletion, e, recoverySide);
                        TriggerOnCalculate(e, recoverySide, resilienceScore);


                        ResetMonitoring();
                    }
                    else if (_depletionStateHolder.IsTimeout())
                    {
                        //never recovered
                        // => means that Resilience Score = 0 (very weak)
                        // => with no direction eLOBSIDE.NONE
                        TriggerOnCalculate(e, eLOBSIDE.NONE, 0);


                        ResetMonitoring();
                    }
                }
            }
            else
            {
                // Update average spread
                _recentSpreads.Add(e.Spread);
                if (_recentSpreads.Count > recentSpreadWindowSize)
                    _recentSpreads.RemoveAt(0);

                
                var depletionState = DepletedSideAndItsPrice(_previousOrderBook.Bids, e.Bids, true, LEVELS_TO_CONSUME);
                if (depletionState.Item1 == eLOBSIDE.NONE)
                    depletionState = DepletedSideAndItsPrice(_previousOrderBook.Asks, e.Asks, false, LEVELS_TO_CONSUME);
                if (depletionState.Item1 != eLOBSIDE.NONE)
                {
                    if (IsSpreadIncreased(e))
                    {
                        //start watching
                        _depletionStateHolder.SetIniOrderBook(e, depletionState.Item1, depletionState.Item2);
                    }
                }
                _previousOrderBook.ShallowUpdateFrom(e);
            }

            ReturnBookItemsToPool(e);
            
        }
        private void QUEUE_onError(Exception ex)
        {
            var _error = $"Unhandled error in the Queue: {ex.Message}";
            log.Error(_error, ex);
            HelperNotificationManager.Instance.AddNotification(this.Name, _error,
                HelprNorificationManagerTypes.ERROR, HelprNorificationManagerCategories.PLUGINS, ex);

            Task.Run(() => HandleRestart(_error, ex));
        }
        protected override void onDataAggregation(BaseStudyModel existing, BaseStudyModel newItem, int counterAggreated)
        {
            //we want to average the aggregations
            existing.Value = ((existing.Value * (counterAggreated - 1)) + newItem.Value) / counterAggreated;
            existing.ValueFormatted = existing.Value.ToString("N1");
            existing.MarketMidPrice = newItem.MarketMidPrice;

            base.onDataAggregation(existing, newItem, counterAggreated);
        }


        private void ReturnBookItemsToPool(OrderBook e)
        {
            if (e.Asks != null)
            {
                foreach (var it in e.Asks)
                    _poolBookItems.Return(it);
            }
            if (e.Bids != null)
            {
                foreach (var it in e.Bids)
                    _poolBookItems.Return(it);
            }
        }
        private Tuple<eLOBSIDE, double> DepletedSideAndItsPrice(CachedCollection<BookItem> previousOrders, CachedCollection<BookItem> currentOrders, bool isBid, int levels)
        {

            if (previousOrders.Count() < levels || currentOrders.Count() < levels)
                return new Tuple<eLOBSIDE, double>(eLOBSIDE.NONE, 0); // Not enough levels to compare

            // Compare the top levels between previous and current snapshots
            for (int i = 0; i < levels; i++)
            {
                if (isBid)
                {
                    if (currentOrders[i].Price < previousOrders[levels - 1].Price)
                    {
                        return new Tuple<eLOBSIDE, double>(eLOBSIDE.BID, previousOrders[0].Price.Value);
                    }
                }
                else
                {
                    if (currentOrders[i].Price > previousOrders[levels - 1].Price)
                    {
                        return new Tuple<eLOBSIDE, double>(eLOBSIDE.ASK, previousOrders[0].Price.Value);
                    }
                }
            }
            return new Tuple<eLOBSIDE, double>(eLOBSIDE.NONE, 0);
        }
        private bool IsSpreadIncreased(OrderBook currentOrderBook)
        {
            if (_previousOrderBook == null)
            {
                return false;
            }

            var averageSpread = _recentSpreads.Average();
            bool spreadCondition = currentOrderBook.Spread > averageSpread * (1 + SPREAD_INCREASED_BY);

            return spreadCondition;
        }

        private bool IsSpreadBackToItsAvg(OrderBook currentOrderBook)
        {
            if (_previousOrderBook == null)
            {
                return false;
            }
            var averageSpread = _recentSpreads.Average();
            bool spreadCondition = currentOrderBook.Spread <= averageSpread;

            return spreadCondition;
        }

        private decimal CalculateMarketResilienceScore(TimeSpan timeSinceLargeTrade, OrderBook currentOrderBook, eLOBSIDE recoveredSide)
        {
            var hasRecoveredOnTheSameSide = _depletionStateHolder.DepletionSide == recoveredSide;

            // Calculate normalized time recovery
            double normalizedTimeRecovery = 1.00 - (timeSinceLargeTrade.TotalMilliseconds / _depletionRecoveryTimeOut.TotalMilliseconds);
            normalizedTimeRecovery = Math.Max(0.0, normalizedTimeRecovery); // Ensure it's not negative

            // Calculate normalized spread recovery
            double normalizedSpreadRecovery = _previousOrderBook.Spread > 0 ? (_previousOrderBook.Spread - currentOrderBook.Spread) / _previousOrderBook.Spread : 0;
            normalizedSpreadRecovery = Math.Max(0.0, Math.Min(1.0, normalizedSpreadRecovery)); // Ensure within range 0-1

            // Calculate normalized depth recovery
            double normalizedDepthRecovery = (double)(currentOrderBook.Asks.Count() + currentOrderBook.Bids.Count()) / (_previousOrderBook.Asks.Count() + _previousOrderBook.Bids.Count());
            normalizedDepthRecovery = Math.Max(0.0, Math.Min(1.0, normalizedDepthRecovery)); // Ensure within range 0-1

            // Weighted average of the metrics
            double weightTime = 0.5; // Assign a weight to time recovery (can be adjusted)
            double weightDepth = 0.1; // Assign a weight to depth recovery (can be adjusted)
            double weightSpread = 0.4;

            double baseScore = weightTime * normalizedTimeRecovery
                               + weightDepth * normalizedDepthRecovery
                               + weightSpread * normalizedSpreadRecovery;

            // Apply bias based on recovery side
            double bias = hasRecoveredOnTheSameSide ? 0.5 : -0.5;
            double resilienceScore = baseScore + bias;

            // Normalize the score to be between 0 and 1 without clipping
            resilienceScore = hasRecoveredOnTheSameSide
                ? 0.5 + (baseScore * 0.5)
                : 0.5 + baseScore;
            resilienceScore = Math.Min(1, Math.Max(0, resilienceScore));
            return (decimal)resilienceScore;
        }

        private eORDERSIDE CalculateMarketResilienceBias(eLOBSIDE recoveredSide, decimal resilienceScore)
        {
            // if "Market Resilience" < 0.3 weak resilience
            // if "Market Resilience" > 0.7 strong resilience


            // if strong resilience AND recovered on the same side => Bias on the opposite direction as the recovery
            // if weak resilience AND recovered on the opposite side => Bias on the same direction as the recovery

            if (recoveredSide == eLOBSIDE.NONE)
                return eORDERSIDE.None;

            var _biasSide = eORDERSIDE.None;
            if (resilienceScore < 0.3m)
            {
                if (_depletionStateHolder.DepletionSide == eLOBSIDE.BID && _depletionStateHolder.DepletionSide != recoveredSide)
                    _biasSide = eORDERSIDE.Sell;
                if (_depletionStateHolder.DepletionSide == eLOBSIDE.ASK && _depletionStateHolder.DepletionSide != recoveredSide)
                    _biasSide = eORDERSIDE.Buy;
            }
            else if (resilienceScore < 0.7m)
            {
                if (_depletionStateHolder.DepletionSide == eLOBSIDE.BID && _depletionStateHolder.DepletionSide == recoveredSide)
                    _biasSide = eORDERSIDE.Buy;
                if (_depletionStateHolder.DepletionSide == eLOBSIDE.ASK && _depletionStateHolder.DepletionSide == recoveredSide)
                    _biasSide = eORDERSIDE.Sell;
            }

            return _biasSide;
        }
        private void TriggerOnCalculate(OrderBook currentOrderBook, eLOBSIDE recoveredSide, decimal resilienceScore)
        {

            var _biasSide = CalculateMarketResilienceBias(recoveredSide, resilienceScore);

            var newItem = new BaseStudyModel()
            {
                Value = resilienceScore,
                ValueFormatted = resilienceScore.ToString("N1"),
                Tooltip = "",
                Timestamp = HelperTimeProvider.Now,
                MarketMidPrice = (decimal)currentOrderBook.MidPrice,
                Tag = _biasSide.ToString()
            };
            AddCalculation(newItem);
        }
        private void ResetMonitoring()
        {
            _depletionStateHolder.Reset();
            _previousOrderBook = new OrderBook();
        }





        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    _QUEUE.Dispose();

                    _previousOrderBook?.Dispose();

                    /*_POOL_OB?.Dispose();
                    _objectPool_BookItem?.Dispose();*/
                    base.Dispose();
                }

            }
        }
        protected override void LoadSettings()
        {
            _settings = LoadFromUserSettings<PlugInSettings>();
            if (_settings == null)
            {
                InitializeDefaultSettings();
            }
            if (_settings.Provider == null) //To prevent back compability with older setting formats
            {
                _settings.Provider = new Provider();
            }
        }
        protected override void SaveSettings()
        {
            SaveToUserSettings(_settings);
        }
        protected override void InitializeDefaultSettings()
        {
            _settings = new PlugInSettings()
            {
                Symbol = "",
                Provider = new Provider(),
                AggregationLevel = AggregationLevel.Ms500
            };
            SaveToUserSettings(_settings);
        }
        public override object GetUISettings()
        {
            PluginSettingsView view = new PluginSettingsView();
            PluginSettingsViewModel viewModel = new PluginSettingsViewModel(CloseSettingWindow);
            viewModel.SelectedSymbol = _settings.Symbol;
            viewModel.SelectedProviderID = _settings.Provider.ProviderID;
            viewModel.AggregationLevelSelection = _settings.AggregationLevel;

            viewModel.UpdateSettingsFromUI = () =>
            {
                _settings.Symbol = viewModel.SelectedSymbol;
                _settings.Provider = viewModel.SelectedProvider;
                _settings.AggregationLevel = viewModel.AggregationLevelSelection;

                SaveSettings();


                //run this because it will allow to restart with the new values
                Task.Run(async () => await HandleRestart($"{this.Name} is starting (from reloading settings).", null, true));

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
