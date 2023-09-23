using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.DataTradeRetriever
{
    public interface IDataTradeRetriever
    {
        event EventHandler<IEnumerable<Order>> OnInitialLoad;
        event EventHandler<IEnumerable<Order>> OnDataReceived;
        DateTime? SessionDate { get; set; }

        ReadOnlyCollection<Order> Orders { get; }
        ReadOnlyCollection<Position> Positions { get; }
    }
}
