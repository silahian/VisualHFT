using System.Collections.Concurrent;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class OrderBookSubscriberBuffer
    {
        public BlockingCollection<OrderBook> Buffer { get; } = new BlockingCollection<OrderBook>();
        public Action<OrderBook> Processor => _processor;

        private Action<OrderBook> _processor;
        public OrderBookSubscriberBuffer(Action<OrderBook> processor)
        {
            _processor = processor;
            Task.Run(Process);
        }

        private void Process()
        {
            foreach (var book in Buffer.GetConsumingEnumerable())
            {
                _processor(book);
            }
        }

        public void Add(OrderBook book) => Buffer.Add(book);

        public int Count => Buffer.Count;
    }
}
