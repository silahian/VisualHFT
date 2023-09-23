using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using VisualHFT.Helpers;
using VisualHFT.Model;
using QuickFix.Fields;

namespace VisualHFT.Studies
{
    public class MarketResilienceStudy : IDisposable
    {
        private bool _disposed = false;
        private string _symbol;
        private int _providerId;
        private AggregatedCollection<BaseStudyModel> _rollingValues;
        private AggregationLevel _aggregationLevel;
        private decimal _marketMidPrice;

        private int LEVELS_TO_CONSUME = 3;
        private double SPREAD_INCREASED_BY = 1.0; //by 100%
        private OrderBook _previousOrderBook;
        private Stopwatch _largeTradeStopwatch;
        private Trade _lastTrade;

        private List<BookItem> CONDITION1_LEVELS_CONSUMED;
        //private int     CONDITION1_LEVELS_CONSUMED = 0;
        private bool    CONDITION2_LEVELS_CONSUMED_AT_BID;
        private double  CONDITION3_SPREAD_INCREASE = 0;

        private decimal _resilienceValue;
        private AggregatedCollection<double> _recentSpreads;
        private const int recentSpreadWindowSize = 50;



        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;
        public event EventHandler<BaseStudyModel> OnRollingAdded;
        public event EventHandler<BaseStudyModel> OnRollingUpdated;
        public event EventHandler<int> OnRollingRemoved;

        public MarketResilienceStudy(string symbol, int providerId, AggregationLevel aggregationLevel, int rollingWindowSize = 50, int levelsToConsume = 3, double spread_increase = 1.0)
        {
            if (string.IsNullOrEmpty(symbol)) throw new Exception("Symbol cannot be null or empty.");

            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;

            _symbol = symbol;
            _providerId = providerId;
            _aggregationLevel = aggregationLevel;
            LEVELS_TO_CONSUME = levelsToConsume;
            SPREAD_INCREASED_BY = spread_increase;

            _recentSpreads = new AggregatedCollection<double>(AggregationLevel.None, recentSpreadWindowSize, null, null);
            _largeTradeStopwatch = new Stopwatch();

            _rollingValues = new AggregatedCollection<BaseStudyModel>(aggregationLevel, rollingWindowSize, x => x.Timestamp, AggregateData);
            _rollingValues.OnRemoved += _rollingValues_OnRemoved;
        }

        ~MarketResilienceStudy()
        {
            Dispose(false);
        }

        private static void AggregateData(BaseStudyModel existing, BaseStudyModel newItem)
        {
            existing.Timestamp = newItem.Timestamp;
            existing.Value = newItem.Value;
        }

        public IReadOnlyList<BaseStudyModel> Data => _rollingValues.AsReadOnly();
        public AggregationLevel AggregationLevel
        {
            get => _aggregationLevel;
            set => _aggregationLevel = value;
        }

        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            if (e == null) return;
            if (_providerId != e.ProviderID || _symbol == "-- All symbols --" || _symbol != e.Symbol) return;

            if (!e.LoadData())
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

        private void TRADES_OnDataReceived(object sender, Trade e)
        {
            if (e == null) return;
            if (_providerId != e.ProviderId || _symbol == "-- All symbols --" || _symbol != e.Symbol) return;

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
            var newItem = new BaseStudyModel() { 
                Value = _resilienceValue, 
                ValueFormatted = _resilienceValue.ToString("N1"),
                Timestamp = DateTime.Now, 
                MarketMidPrice = (decimal)currentOrderBook.MidPrice 
            };
            bool addSuccess = _rollingValues.Add(newItem);
            if (addSuccess)
                OnRollingAdded?.Invoke(this, newItem);
            else
                OnRollingUpdated?.Invoke(this, newItem);


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


        private void _rollingValues_OnRemoved(object sender, int e)
        {
            OnRollingRemoved?.Invoke(this, e);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _providerId = 0;
                    _symbol = "";

                    HelperCommon.LIMITORDERBOOK.OnDataReceived -= LIMITORDERBOOK_OnDataReceived;
                    HelperCommon.TRADES.OnDataReceived -= TRADES_OnDataReceived;
                    _largeTradeStopwatch.Reset();
                    _largeTradeStopwatch = null;

                    _rollingValues.OnRemoved -= _rollingValues_OnRemoved;
                    _rollingValues.Clear();
                    _rollingValues = null;

                    if (CONDITION1_LEVELS_CONSUMED != null)
                    {
                        CONDITION1_LEVELS_CONSUMED.Clear();
                        CONDITION1_LEVELS_CONSUMED = null;
                    }
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
