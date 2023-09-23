using System;
using System.Collections.Generic;
using Confluent.Kafka;
using VisualHFT.Model;

namespace VisualHFT.DataRetriever
{
    public class KafkaDataRetriever : IDataRetriever
    {
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private IConsumer<Ignore, string> _consumer;
        public event EventHandler<DataEventArgs> OnDataReceived;

        public KafkaDataRetriever(string bootstrapServers, string topic)
        {
            _bootstrapServers = bootstrapServers;
            _topic = topic;
        }

        public void Start()
        {
            var config = new ConsumerConfig
            {
                GroupId = "visualhft-group",
                BootstrapServers = _bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _consumer.Subscribe(_topic);

            while (true)
            {
                try
                {
                    var consumeResult = _consumer.Consume();
                    HandleMessage(consumeResult.Message.Value);
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume error: {e.Error.Reason}");
                }
            }
        }

        public void Stop()
        {
            _consumer.Close();
            _consumer.Dispose();
        }

        private void HandleMessage(string message)
        {
            // Process the received message
            var model = new OrderBook();

            // parse message and populate 'model'



            // Raise the OnDataReceived event
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = "Market", RawData = message, ParsedModel = model });

            var provider = new VisualHFT.ViewModel.Model.Provider() { LastUpdated = DateTime.Now, ProviderID = 2, ProviderName = "Kafka", Status = eSESSIONSTATUS.BOTH_CONNECTED };
            // Raise the OnDataReceived event
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = "HeartBeats", RawData = message, ParsedModel = model });
        }
    }
}
