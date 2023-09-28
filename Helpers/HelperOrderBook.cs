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
    public class HelperOrderBook
    {
        protected ConcurrentQueue<OrderBook> _DataQueue = new ConcurrentQueue<OrderBook>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Task _processingTask;

        public event EventHandler<OrderBook> OnDataReceived;

        public HelperOrderBook()
        {
            _processingTask = Task.Run(() => ProcessQueue(), _cancellationTokenSource.Token);
        }
        ~HelperOrderBook()
        {
            _cancellationTokenSource.Cancel();
            _processingTask.Wait();
        }

        private void ProcessQueue()
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
                    Console.WriteLine("HelperOrderBook QUEUE is way behind: " + _DataQueue.Count);
                }

                while (_DataQueue.TryDequeue(out var ob))
                    data.Add(ob);

                if (data.Any())
                    RaiseOnDataReceived(data);

                // Wait for the next iteration
                Task.Delay(1).Wait();
            }
        }


        protected virtual void RaiseOnDataReceived(List<OrderBook> books)
        {
            EventHandler<OrderBook> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach (var ob in books)
                    _handler(this, ob);
            }
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
