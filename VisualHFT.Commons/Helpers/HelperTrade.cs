using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class HelperTrade
    {

        private List<Action<Trade>> _subscribers = new List<Action<Trade>>();

        private readonly object _lockObj = new object();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly HelperTrade instance = new HelperTrade();
        public static HelperTrade Instance => instance;



        public HelperTrade()
        {

        }
        ~HelperTrade()
        {
        }

        public void Subscribe(Action<Trade> processor)
        {
            lock (_lockObj)
            {
                _subscribers.Add(processor);
            }
        }

        public void Unsubscribe(Action<Trade> processor)
        {
            lock (_lockObj)
            {
                _subscribers.Remove(processor);
            }
        }

        private void DispatchToSubscribers(Trade trade)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber(trade);
            }
        }



        public void UpdateData(Trade trade)
        {
            DispatchToSubscribers(trade);
        }
        public void UpdateData(IEnumerable<Trade> trades)
        {
            foreach (var e in trades)
            {
                DispatchToSubscribers(e);
            }
        }
    }
}