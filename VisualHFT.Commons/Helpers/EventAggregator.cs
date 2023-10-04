using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class EventAggregator
    {
        private static readonly EventAggregator instance = new EventAggregator();
        public static EventAggregator Instance => instance;

        public event EventHandler<OrderBook> OnOrderBookDataReceived;

        public void PublishOrderBookDataReceived(object sender, OrderBook data)
        {
            OnOrderBookDataReceived?.Invoke(sender, data);
        }
    }
}
