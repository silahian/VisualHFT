using VisualHFT.Model;
using System.Collections.Concurrent;
using System.Globalization;
using VisualHFT.Commons.Pools;
using VisualHFT.Commons.SubscriberBuffers;

namespace VisualHFT.Helpers
{


    public sealed class HelperOrderBook : IOrderBookHelper
    {
        protected BlockingCollection<OrderBook> _DataQueue = new BlockingCollection<OrderBook>(new ConcurrentQueue<OrderBook>());
        private List<OrderBookSubscriberBuffer> _subscribers = new List<OrderBookSubscriberBuffer>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _processingTask;
        private readonly object _lockObj = new object();
        private readonly ObjectPool<OrderBook> orderBookPool = new ObjectPool<OrderBook>();


        // This timer will be used for performance monitoring
        private readonly System.Timers.Timer _monitoringTimer;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly HelperOrderBook instance = new HelperOrderBook();
        public static HelperOrderBook Instance => instance;

        private readonly ObjectPool<OrderBook> orerBookPool = new ObjectPool<OrderBook>();//pool of Trade objects


        private HelperOrderBook()
        {
            _processingTask = Task.Run(async () => await ProcessQueueAsync(), _cancellationTokenSource.Token);

            // Set up the performance monitoring timer
            _monitoringTimer = new System.Timers.Timer(5000); // Check every 5 seconds
            _monitoringTimer.Elapsed += MonitorSubscriberBuffers;
            _monitoringTimer.Start();
        }
        ~HelperOrderBook()
        {
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
        }



        public void Subscribe(Action<OrderBook> processor)
        {
            lock (_lockObj)
            {
                _subscribers.Add(new OrderBookSubscriberBuffer(processor));
            }
        }

        public void Unsubscribe(Action<OrderBook> processor)
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
                    log.Warn($"OrderBook Subscriber buffer is growing large: {subscriber.Count}");
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
                        log.Warn($"HelperOrderBook QUEUE is way behind: {_DataQueue.Count}");
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
        private void DispatchToSubscribers(OrderBook book)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Add(book);
            }
        }
        public void UpdateData(IEnumerable<OrderBook> data)
        {
            foreach (var e in data)
            {
                var pooledOrderBook = orderBookPool.Get();
                e.CopyTo(pooledOrderBook);
                _DataQueue.Add(pooledOrderBook);
            }
        }
    }
}
