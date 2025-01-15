using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public interface IOrderBookHelper
    {
        //event EventHandler<OrderBook> OnDataReceived;
        void Subscribe(Action<OrderBook> processor);
        void UpdateData(IEnumerable<OrderBook> data);
    }
}
