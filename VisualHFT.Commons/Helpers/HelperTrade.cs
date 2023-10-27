using System.Collections.Concurrent;
using System.Globalization;
using VisualHFT.Commons.Pools;
using VisualHFT.Commons.SubscriberBuffers;
using VisualHFT.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VisualHFT.Helpers
{
    public class HelperTrade
    {
        protected BlockingCollection<Trade> _DataQueue = new BlockingCollection<Trade>(new ConcurrentQueue<Trade>());
        private List<TradeSubscriberBuffer> _subscribers = new List<TradeSubscriberBuffer>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _processingTask;
        private readonly object _lockObj = new object();
        private readonly ObjectPool<Trade> orderBookPool = new ObjectPool<Trade>();

        // This timer will be used for performance monitoring
        private readonly System.Timers.Timer _monitoringTimer;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly HelperTrade instance = new HelperTrade();
        public static HelperTrade Instance => instance;

        private readonly ObjectPool<Trade> tradePool = new ObjectPool<Trade>();//pool of Trade objects


        public HelperTrade()
        {
            _processingTask = Task.Run(async () => await ProcessQueueAsync(), _cancellationTokenSource.Token);

            // Set up the performance monitoring timer
            _monitoringTimer = new System.Timers.Timer(5000); // Check every 5 seconds
            _monitoringTimer.Elapsed += MonitorSubscriberBuffers;
            _monitoringTimer.Start();
        }
        ~HelperTrade()
        {
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
        }

        public void Subscribe(Action<Trade> processor)
        {
            lock (_lockObj)
            {
                _subscribers.Add(new TradeSubscriberBuffer(processor));
            }
        }

        public void Unsubscribe(Action<Trade> processor)
        {
            lock (_lockObj)
            {
                var bufferToRemove = _subscribers.FirstOrDefault(buffer => buffer.Processor == processor);
                if (bufferToRemove != null)
                {
                    _subscribers.Remove(bufferToRemove);
                    bufferToRemove.Buffer.CompleteAdding();
                }
            }
        }
        private void MonitorSubscriberBuffers(object? sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var subscriber in _subscribers)
            {
                if (subscriber.Count > 500)  // or some threshold value
                {
                    log.Warn($"Trade Subscriber buffer is growing large: {subscriber.Count}");
                    // Additional actions as needed: Pause, Alert, Disconnect
                }
            }
        }
        private async Task ProcessQueueAsync()
        {
            Thread.CurrentThread.IsBackground = true;

            CultureInfo ci_clone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            ci_clone.NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

            Thread.CurrentThread.CurrentCulture = ci_clone;
            Thread.CurrentThread.CurrentUICulture = ci_clone;
            List<OrderBook> data = new List<OrderBook>();

            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_DataQueue.Count > 500)
                    {
                        log.Warn($"HelperTrade QUEUE is way behind: {_DataQueue.Count}");
                    }

                    var ob = _DataQueue.Take();
                    DispatchToSubscribers(ob);


                    // Wait for the next iteration
                    await Task.Delay(0);
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }
        private void DispatchToSubscribers(Trade trade)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Add(trade);
            }
        }



        public void UpdateData(IEnumerable<Trade> trades)
        {
            foreach (var e in trades)
            {
                var pooledOrderBook = orderBookPool.Get();
                e.CopyTo(pooledOrderBook);
                _DataQueue.Add(pooledOrderBook);
            }
        }
    }
}
