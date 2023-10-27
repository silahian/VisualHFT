using System.Collections.Concurrent;
using VisualHFT.Model;

namespace VisualHFT.Commons.SubscriberBuffers
{
    public class TradeSubscriberBuffer
    {
        public BlockingCollection<Trade> Buffer { get; } = new BlockingCollection<Trade>();
        public Action<Trade> Processor => _processor;

        private Action<Trade> _processor;
        public TradeSubscriberBuffer(Action<Trade> processor)
        {
            _processor = processor;
            Task.Run(Process);
        }

        private void Process()
        {
            foreach (var trade in Buffer.GetConsumingEnumerable())
            {
                _processor(trade);
            }
        }

        public void Add(Trade book) => Buffer.Add(book);

        public int Count => Buffer.Count;
    }
}
