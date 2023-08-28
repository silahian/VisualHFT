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
        event EventHandler<IEnumerable<OrderVM>> OnInitialLoad;
        event EventHandler<IEnumerable<OrderVM>> OnDataReceived;
        DateTime? SessionDate { get; set; }

        ReadOnlyCollection<OrderVM> Orders { get; }
        ReadOnlyCollection<PositionEx> Positions { get; }
    }
}
