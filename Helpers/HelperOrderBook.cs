using VisualHFT.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace VisualHFT.Helpers
{
    public class HelperOrderBook: IOrderBookHelper
    {
        protected ConcurrentQueue<OrderBook> _DataQueue = new ConcurrentQueue<OrderBook>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _processingTask;

        public event EventHandler<OrderBook> OnDataReceived;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public HelperOrderBook()
        {
            _processingTask = Task.Run(async () => await ProcessQueueAsync(), _cancellationTokenSource.Token);
        }
        ~HelperOrderBook()
        {
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
        }

        private async Task ProcessQueueAsync()
        {
            Thread.CurrentThread.IsBackground = true;

            CultureInfo ci_clone = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            ci_clone.NumberFormat = CultureInfo.GetCultureInfo("en-US").NumberFormat;

            Thread.CurrentThread.CurrentCulture = ci_clone;
            Thread.CurrentThread.CurrentUICulture = ci_clone;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                List<OrderBook> data = new List<OrderBook>();
                if (_DataQueue.Count > 500)
                {
                    log.Warn("HelperOrderBook QUEUE is way behind: " + _DataQueue.Count);
                }

                while (_DataQueue.TryDequeue(out var ob))
                    data.Add(ob);

                if (data.Any())
                    RaiseOnDataReceived(data);

                // Wait for the next iteration
                await Task.Delay(1);
            }
        }
        protected virtual void RaiseOnDataReceived(List<OrderBook> books)
        {
            foreach (var ob in books)
                EventAggregator.Instance.PublishOrderBookDataReceived(this, ob);
        }
        public void UpdateData(IEnumerable<OrderBook> data)
        {
            foreach (var e in data)
            {
                _DataQueue.Enqueue(e);
            }

        }

    }
}
