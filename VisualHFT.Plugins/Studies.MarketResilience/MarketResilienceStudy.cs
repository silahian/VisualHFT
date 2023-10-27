using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VisualHFT.Commons.PluginManager;
using VisualHFT.Commons.Pools;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.PluginManager;
using VisualHFT.Studies.MarketResilience.Model;
using VisualHFT.Studies.MarketResilience.UserControls;
using VisualHFT.Studies.MarketResilience.ViewModel;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies
{
    public class MarketResilienceStudy : BasePluginStudy
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private PlugInSettings _settings;



        private int LEVELS_TO_CONSUME = 3;
        private double SPREAD_INCREASED_BY = 1.0; //by 100%
        private OrderBook _previousOrderBook;
        private Stopwatch _largeTradeStopwatch;
        private Trade _lastTrade;

        private List<BookItem> CONDITION1_LEVELS_CONSUMED;
        //private int     CONDITION1_LEVELS_CONSUMED = 0;
        private bool CONDITION2_LEVELS_CONSUMED_AT_BID;
        private double CONDITION3_SPREAD_INCREASE = 0;

        private decimal _resilienceValue;
        private AggregatedCollection<double> _recentSpreads;
        private const int recentSpreadWindowSize = 50;




        // Event declaration
        public override event EventHandler<decimal> OnAlertTriggered;
        public override event EventHandler<BaseStudyModel> OnCalculated;
        public override event EventHandler<ErrorEventArgs> OnError;

        public override string Name { get; set; } = "Market Resiliecence Study Plugin";
        public override string Version { get; set; } = "1.0.0";
        public override string Description { get; set; } = "Market Resiliecence";
        public override string Author { get; set; } = "VisualHFT";
        public override ISetting Settings { get => _settings; set => _settings = (PlugInSettings)value; }
        public override Action CloseSettingWindow { get; set; }
        public override string TileTitle { get; set; } = "MR";
        public override string TileToolTip { get; set; } = "<b>Market Resilience</b> (MR) is a real-time metric that quantifies how quickly a market rebounds after experiencing a large trade. <br/> It's an invaluable tool for traders to gauge market stability and sentiment.<br/><br/>" +
                "The <b>MR</b> score is a composite index derived from two key market behaviors:<br/>" +
                "1. <b>Spread Recovery:</b> Measures how quickly the gap between buying and selling prices returns to its normal state after a large trade.<br/>" +
                "2. <b>Depth Recovery:</b>  Assesses how fast the consumed levels of the Limit Order Book (LOB) are replenished post-trade.<br/>" +
                "<br/>" +
                "The <b>MR</b> score is the average of these two normalized metrics, ranging from 0 (no recovery) to 1 (full recovery).";

        public MarketResilienceStudy()
        {
            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);
            HelperTrade.Instance.Subscribe(TRADES_OnDataReceived);

            _recentSpreads = new AggregatedCollection<double>(AggregationLevel.None, recentSpreadWindowSize, null, null);
            _largeTradeStopwatch = new Stopwatch();

        }
        ~MarketResilienceStudy()
        {
            Dispose(false);
        }


        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderID || _settings.Symbol != e.Symbol)
                return;

            // Update average spread
            _recentSpreads.Add(e.Spread);


            bool levelsHasBeenConsumed = false;
            bool spreadHasBeenWiden = false;
            if (!_largeTradeStopwatch.IsRunning)
            {
                levelsHasBeenConsumed = PostTradeLOBLevelsConsumed(e);
                spreadHasBeenWiden = PostTradeLOBSpreadIsIncreased(e);

                _previousOrderBook = (OrderBook)e.Clone();
            }
            else
            {
                CalculateResilience(e);
            }
            TriggerOnCalculatedEvent(e);

            // If large trade detected, start resilience calculation
            if (levelsHasBeenConsumed && spreadHasBeenWiden)
            {
                _largeTradeStopwatch = Stopwatch.StartNew();    // Capture the time of the large trade
            }
        }
        private void TRADES_OnDataReceived(Trade e)
        {
            if (e == null)
                return;
            if (_settings.Provider.ProviderID != e.ProviderId || _settings.Symbol != e.Symbol)
                return;

            _lastTrade = e;
        }



        private void CalculateResilience(OrderBook currentOrderBook)
        {
            if (IsDepthRecovered(currentOrderBook))
            {
                TimeSpan timeSinceLargeTrade = _largeTradeStopwatch.Elapsed;
                _resilienceValue = CalculateResilienceValue(timeSinceLargeTrade, currentOrderBook);


                ResetPreTradeState();
            }

        }
        private bool IsDepthRecovered(OrderBook currentOrderBook)
        {
            // Check if the number of levels has recovered
            bool asksHaveRecoveredCount = currentOrderBook.Asks.Count >= LEVELS_TO_CONSUME;
            bool bidsHaveRecoveredCount = currentOrderBook.Bids.Count >= LEVELS_TO_CONSUME;

            // Check if the spread has recovered
            bool spreadRecovered = currentOrderBook.Spread <= _previousOrderBook.Spread * (1 + SPREAD_INCREASED_BY);

            // Create an instance of BookItem to use its comparison logic
            BookItem comparer = new BookItem();

            // Check if the prices of the levels have recovered
            bool asksHaveRecoveredPrice =
                // Scenario 1: Levels have shifted up but count is maintained
                currentOrderBook.Asks.Take(LEVELS_TO_CONSUME).SequenceEqual(_previousOrderBook.Asks.Skip(LEVELS_TO_CONSUME).Take(LEVELS_TO_CONSUME), comparer) ||
                // Scenario 2: Levels have jumped significantly
                !currentOrderBook.Asks.Take(LEVELS_TO_CONSUME).Any(ask => _previousOrderBook.Asks.Any(a => comparer.Equals(a, ask))) ||
                // Scenario 3: Levels have returned to their original state
                currentOrderBook.Asks.Take(LEVELS_TO_CONSUME).SequenceEqual(_previousOrderBook.Asks.Take(LEVELS_TO_CONSUME), comparer);

            return asksHaveRecoveredCount && bidsHaveRecoveredCount && spreadRecovered && asksHaveRecoveredPrice;
        }
        private decimal CalculateResilienceValue(TimeSpan timeSinceLargeTrade, OrderBook currentOrderBook)
        {
            // Define a threshold for maximum acceptable recovery time (e.g., 1 minutes)
            TimeSpan maxRecoveryTime = TimeSpan.FromMinutes(1);

            // Calculate normalized time recovery
            double normalizedTimeRecovery = 1 - (timeSinceLargeTrade.TotalSeconds / maxRecoveryTime.TotalSeconds);
            normalizedTimeRecovery = Math.Max(0, normalizedTimeRecovery); // Ensure it's not negative

            // Calculate normalized depth recovery (as previously discussed)
            double normalizedSpreadRecovery = (_previousOrderBook.Spread - currentOrderBook.Spread) / _previousOrderBook.Spread;
            double normalizedDepthRecovery = (double)(currentOrderBook.Asks.Count + currentOrderBook.Bids.Count) / (_previousOrderBook.Asks.Count + _previousOrderBook.Bids.Count);

            // Weighted average of the metrics
            double weightTime = 0.7; // Assign a weight to time recovery (can be adjusted)
            double weightDepth = 0.3; // Assign a weight to depth recovery (can be adjusted)

            decimal resilienceValue = (decimal)(weightTime * normalizedTimeRecovery + weightDepth * normalizedDepthRecovery);

            return resilienceValue;
        }
        private void TriggerOnCalculatedEvent(OrderBook currentOrderBook)
        {
            var newItem = new BaseStudyModel()
            {
                Value = _resilienceValue,
                ValueFormatted = _resilienceValue.ToString("N1"),
                Timestamp = DateTime.Now,
                MarketMidPrice = (decimal)currentOrderBook.MidPrice
            };
            OnCalculated?.Invoke(this, newItem);
        }
        private void ResetPreTradeState()
        {
            _previousOrderBook = null;
            CONDITION1_LEVELS_CONSUMED = null;
            CONDITION2_LEVELS_CONSUMED_AT_BID = false;
            CONDITION3_SPREAD_INCREASE = 0;
            _largeTradeStopwatch.Stop();
        }
        private bool PostTradeLOBLevelsConsumed(OrderBook currentOrderBook)
        {
            if (_previousOrderBook == null)
            {
                return false;
            }

            int consumedAskLevels = _previousOrderBook.Asks
                .Take(LEVELS_TO_CONSUME)
                .Count(ask => !currentOrderBook.Asks.Any(a => a.Price == ask.Price));

            int consumedBidLevels = _previousOrderBook.Bids
                .Take(LEVELS_TO_CONSUME)
                .Count(bid => !currentOrderBook.Bids.Any(b => b.Price == bid.Price));

            if (consumedAskLevels >= LEVELS_TO_CONSUME)
            {
                CONDITION1_LEVELS_CONSUMED = _previousOrderBook.Asks.Take(LEVELS_TO_CONSUME).ToList();
                CONDITION2_LEVELS_CONSUMED_AT_BID = false;
                return true;
            }
            else if (consumedBidLevels >= LEVELS_TO_CONSUME)
            {
                CONDITION1_LEVELS_CONSUMED = _previousOrderBook.Bids.Take(LEVELS_TO_CONSUME).ToList();
                CONDITION2_LEVELS_CONSUMED_AT_BID = true;
                return true;
            }

            return false;
        }
        private bool PostTradeLOBSpreadIsIncreased(OrderBook currentOrderBook)
        {
            if (_previousOrderBook == null)
            {
                return false;
            }

            var averageSpread = _recentSpreads.ToList().Average();
            bool spreadCondition = currentOrderBook.Spread > averageSpread * (1 + SPREAD_INCREASED_BY);

            if (spreadCondition)
            {
                CONDITION3_SPREAD_INCREASE = (currentOrderBook.Spread - averageSpread) / currentOrderBook.Spread;
            }

            return spreadCondition;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);
                    HelperTrade.Instance.Unsubscribe(TRADES_OnDataReceived);
                    _largeTradeStopwatch.Reset();
                    _largeTradeStopwatch = null;

                    if (CONDITION1_LEVELS_CONSUMED != null)
                    {
                        CONDITION1_LEVELS_CONSUMED.Clear();
                        CONDITION1_LEVELS_CONSUMED = null;
                    }
                }
                _disposed = true;
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
                AggregationLevel = AggregationLevel.Automatic
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

            };
            // Display the view, perhaps in a dialog or a new window.
            view.DataContext = viewModel;
            return view;
        }

    }
}
