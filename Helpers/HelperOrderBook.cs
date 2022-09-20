using VisualHFT.Model;
using VisualHFT.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperOrderBook: ConcurrentDictionary<string, OrderBook>
    {
        protected ConcurrentQueue<OrderBook> _DataQueue = new ConcurrentQueue<OrderBook>();
        protected System.Timers.Timer _queueTimer;

        public event EventHandler<OrderBook> OnDataReceived;

        public HelperOrderBook()
        {
            _queueTimer = new System.Timers.Timer(); //TimeSpan.FromMilliseconds(30), DispatcherPriority.Render, _queueTimer_Tick, );
            _queueTimer.Interval = 30;
            _queueTimer.Elapsed += _queueTimer_Tick;
            _queueTimer.Start();
        }
        ~HelperOrderBook()
        {}

        private void _queueTimer_Tick(object sender, EventArgs e)
        {
            List<OrderBook> data = new List<OrderBook>();
            if (_DataQueue.Count > 100)
                return;

            while (_DataQueue.TryDequeue(out var ob))
            {
                data.Add(ob);
            }

            if (data.Any())
                RaiseOnDataReceived(data);

        }


        protected virtual void RaiseOnDataReceived(List<OrderBook> books)
        {
            EventHandler<OrderBook> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach(var ob in books)
                    _handler(this, ob);
                //Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {					
				//}));
            }
        }



        public void UpdateData(IEnumerable<OrderBook> data)
        {
            foreach (var e in data)
            {
                if (UpdateData(e))
                {
                    _DataQueue.Enqueue(e);
                }
            }

        }

        public bool UpdateData(OrderBook book)
        {
            if (book != null)
            {
                if (!this.ContainsKey(book.KEY))
                {
                    return this.TryAdd(book.KEY, book);
                }
                else
                {
                    this[book.KEY] = book;
                    return true;
                }
            }
            return false;
        }


    }
}
