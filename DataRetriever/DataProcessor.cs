using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;

namespace VisualHFT.DataRetriever
{
    public class DataProcessor
    {
        private IDataRetriever _dataRetriever;
        BlockingCollection<DataEventArgs> _dataQueue = new BlockingCollection<DataEventArgs>(new ConcurrentQueue<DataEventArgs>());
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private object _LOCK_SYMBOLS = new object();
        private const int MAX_QUEUE_SIZE = 10000; // Define a threshold for max queue size
        private const int BACK_PRESSURE_DELAY = 300; // Delay in milliseconds to apply back pressure
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DataProcessor(IDataRetriever dataRetriever)
        {
            _dataRetriever = dataRetriever;
            _dataRetriever.OnDataReceived += EnqueueData;
            StartProcessing();
        }

        private void EnqueueData(object sender, DataEventArgs e)
        {
            if (_dataQueue.Count >= MAX_QUEUE_SIZE)
            {
                // Apply back pressure by delaying further data ingestion
                Task.Delay(BACK_PRESSURE_DELAY).Wait();
            }
            _dataQueue.Add(e);
        }

        private void StartProcessing()
        {
            Task.Run(() =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        foreach (var data in _dataQueue.GetConsumingEnumerable())
                        {
                            if (_dataQueue.Count > 1000)
                                log.Warn("WARNING: DataProcessor Queue is behind: " + _dataQueue.Count.ToString());
                            if (data != null)
                                HandleData(data);
                        }                                                
                    }
                    catch (Exception ex)
                    {
                        log.Error("ERROR: " + ex.ToString());
                    }
                }
            });
        }

        private void HandleData(DataEventArgs e)
        {
            switch (e.DataType)
            {
                case "Market":
                    var orderBook = e.ParsedModel as IEnumerable<OrderBook>;
                    if (orderBook != null)
                    {
                        var allSymbols = orderBook.Select(x => x.Symbol).Distinct();
                        ParseSymbols(allSymbols);
                        ParseOrderBook(orderBook);
                    }
                    break;
                case "ActiveOrders":
                    ParseActiveOrders(e.ParsedModel as List<VisualHFT.Model.Order>);
                    break;
                case "Strategies":
                    ParseActiveStrategies(e.ParsedModel as List<StrategyVM>);
                    break;
                case "Exposures":
                    ParseExposures(e.ParsedModel as List<Exposure>);
                    break;
                case "HeartBeats":
                    ParseHeartBeat(e.ParsedModel as List<VisualHFT.Model.Provider>);
                    break;
                case "Trades":
                    ParseTrades(e.ParsedModel as List<Trade>);
                    break;
                default:
                    break;
            }
        }
        #region Parsing Methods        
        private void ParseSymbols(IEnumerable<string> symbols)
        {
            if (HelperCommon.ALLSYMBOLS == null)
                HelperCommon.ALLSYMBOLS = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (Application.Current == null)
                return;
            foreach (var s in symbols)
            {
                if (!HelperCommon.ALLSYMBOLS.Contains(s))
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                    {
                        if (!HelperCommon.ALLSYMBOLS.Contains(s))
                            HelperCommon.ALLSYMBOLS.Add(s);
                    }));
                }
            }

        }
        private void ParseOrderBook(IEnumerable<OrderBook> orderBooks)
        {
            HelperCommon.LIMITORDERBOOK.UpdateData(orderBooks);
        }
        private void ParseExposures(IEnumerable<Exposure> exposures)
        {
            HelperCommon.EXPOSURES.UpdateData(exposures);
        }
        private void ParseActiveOrders(IEnumerable<VisualHFT.Model.Order> activeOrders)
        {
            HelperCommon.ACTIVEORDERS.UpdateData(activeOrders.ToList());
        }
        private void ParseActiveStrategies(IEnumerable<StrategyVM> activeStrategies)
        {
            HelperCommon.ACTIVESTRATEGIES.UpdateData(activeStrategies.ToList());
        }
        private void ParseStrategyParams(string data)
        {
            HelperCommon.STRATEGYPARAMS.RaiseOnDataUpdateReceived(data);

        }
        private void ParseHeartBeat(IEnumerable<VisualHFT.Model.Provider> providers)
        {
            HelperCommon.PROVIDERS.UpdateData(providers.ToList());
        }
        private void ParseTrades(IEnumerable<Trade> trades)
        {
            HelperCommon.TRADES.UpdateData(trades.ToList());
        }
        #endregion


    }

}
