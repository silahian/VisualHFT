using System.Collections.ObjectModel;
using VisualHFT.Model;

namespace VisualHFT.DataTradeRetriever
{
    public interface IDataTradeRetriever
    {
        event EventHandler<IEnumerable<Order>> OnInitialLoad;
        event EventHandler<IEnumerable<Order>> OnDataReceived;
        event EventHandler<Order> OnDataUpdated;

        DateTime? SessionDate { get; set; }

        ReadOnlyCollection<Order> Orders { get; }
        ReadOnlyCollection<Position> Positions { get; }

        void AddOrder(Order? order);
    }
}
