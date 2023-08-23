using System;
using System.Collections.Generic;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using VisualHFT.Model;

namespace VisualHFT.DataRetriever
{
    public class ZeroMQDataRetriever : IDataRetriever
    {
        private readonly string _connectionString;
        private SubscriberSocket _subscriber;
        public event EventHandler<DataEventArgs> OnDataReceived;

        public ZeroMQDataRetriever(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Start()
        {
            _subscriber = new SubscriberSocket();
            _subscriber.Connect(_connectionString);
            _subscriber.Subscribe(""); // Subscribe to all messages

            // Start listening for messages in a separate thread
            _subscriber.ReceiveReady += (s, e) =>
            {
                var message = e.Socket.ReceiveFrameString();
                HandleMessage(message);
            };

            // You can use NetMQ's built-in Poller to continuously listen for messages
            using (var poller = new NetMQPoller { _subscriber })
            {
                poller.Run();
            }
        }

        public void Stop()
        {
            _subscriber.Close();
            _subscriber.Dispose();
        }

        private void HandleMessage(string message)
        {
            // Process the received message
            var model = new OrderBook
            {
                Bids = new List<BookItem>(),
                Asks = new List<BookItem>()
            };

            // parse message and populate 'model'



            // Raise the OnDataReceived event
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = "Market", RawData = message, ParsedModel = model });



            var provider = new ProviderEx() { LastUpdated = DateTime.Now, ProviderID = 1, ProviderName = "ZeroMQ", Status = eSESSIONSTATUS.BOTH_CONNECTED };
            // Raise the OnDataReceived event
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = "HeartBeats", RawData = message, ParsedModel = model });

        }
    }
}
