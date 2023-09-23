using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using QuickFix.FIX44;
using System;


namespace VisualHFT.DataRetriever
{
    using QuickFix;
    using QuickFix.Fields;
    using QuickFix.Transport;
    using QuickFix.FIX44;
    using System;
    using System.ComponentModel.DataAnnotations;
    using VisualHFT.Model;
    using System.Collections.Generic;

    public class FIXDataRetriever : IDataRetriever, IApplication
    {
        private SessionSettings _settings;
        private IMessageStoreFactory _storeFactory;
        private ILogFactory _logFactory;
        private IInitiator _initiator;
        public event EventHandler<DataEventArgs> OnDataReceived;

        public FIXDataRetriever()
        {
            // Configuration settings
            _settings = new SessionSettings();
            var sessionID = new SessionID("FIX.4.4", "YOUR_SENDER_COMP_ID", "YOUR_TARGET_COMP_ID");
            var dictionary = new Dictionary();
            dictionary.SetString("ConnectionType", "initiator");
            dictionary.SetString("ReconnectInterval", "2");
            dictionary.SetString("FileLogPath", "log");
            dictionary.SetString("StartTime", "00:00:00");
            dictionary.SetString("EndTime", "00:00:00");
            dictionary.SetString("HeartBtInt", "30");
            dictionary.SetString("SocketConnectHost", "YOUR_FIX_SERVER_HOST");
            dictionary.SetString("SocketConnectPort", "YOUR_FIX_SERVER_PORT");
            _settings.Set(sessionID, dictionary);

            _storeFactory = new FileStoreFactory(_settings);
            _logFactory = new FileLogFactory(_settings);
            _initiator = new SocketInitiator(this, _storeFactory, _settings, _logFactory);
        }

        public void Start()
        {
            if (!_initiator.IsLoggedOn)
            {
                _initiator.Start();
            }
        }

        public void Stop()
        {
            if (_initiator.IsLoggedOn)
            {
                _initiator.Stop();
            }
        }

        // IApplication methods
        public void OnCreate(SessionID sessionId)
        {
        }

        public void OnLogout(SessionID sessionId)
        {
        }

        public void OnLogon(SessionID sessionId)
        {
            // Send market data request after logging in
            SendMarketDataRequest();
        }

        public void ToAdmin(QuickFix.Message message, SessionID sessionId)
        {
            // Handle messages sent to the FIX server (e.g., logon messages)
        }

        public void ToApp(QuickFix.Message message, SessionID sessionId)
        {
            // Handle application-level messages sent to the FIX server
        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionId)
        {
            // Handle administrative messages received from the FIX server (e.g., heartbeats)
        }

        public void FromApp(QuickFix.Message message, SessionID sessionId)
        {
            if (message is MarketDataSnapshotFullRefresh snapshot)
            {
                HandleMarketDataSnapshot(snapshot);
            }
            else if (message is QuickFix.FIX44.Heartbeat heartbeatMessage)
            {
                Console.WriteLine("Received Heartbeat from: " + sessionId.ToString());
                HandleHeartBeat();
            }            
            else if (message is QuickFix.FIX44.TestRequest testRequestMessage) // Check if the message is a Test Request
            {
                var testReqID = testRequestMessage.TestReqID.getValue();
                var responseHeartbeat = new QuickFix.FIX44.Heartbeat();
                responseHeartbeat.SetField(new QuickFix.Fields.TestReqID(testReqID));
                Session.SendToTarget(responseHeartbeat, sessionId);
            }
        }


        private void SendMarketDataRequest()
        {
            var marketDataRequest = new MarketDataRequest();

            // Set unique ID for the request
            marketDataRequest.Set(new MDReqID(Guid.NewGuid().ToString()));

            // Request type: 0 = Snapshot
            marketDataRequest.Set(new SubscriptionRequestType('0'));

            // Market depth: 0 = Full book
            marketDataRequest.Set(new MarketDepth(0));

            // Add symbols or other criteria for which you want the snapshot
            var noRelatedSymGroup = new MarketDataRequest.NoRelatedSymGroup();
            noRelatedSymGroup.Set(new Symbol("EUR/USD")); // Replace with your desired symbol
            marketDataRequest.AddGroup(noRelatedSymGroup);

            // Send the message
            Session.SendToTarget(marketDataRequest);
        }
        private void HandleMarketDataSnapshot(MarketDataSnapshotFullRefresh snapshot)
        {
            int? decimalPlaces = null;

            // Extract data from the snapshot
            var symbol = snapshot.Get(new Symbol()).getValue();
            var _bids = new List<BookItem>();
            var _asks = new List<BookItem>();

            // Iterate through the repeating groups for market data entries
            var noMDEntries = snapshot.GetInt(Tags.NoMDEntries);
            for (int i = 1; i <= noMDEntries; i++)
            {
                var group = snapshot.GetGroup(i, Tags.NoMDEntries);
                var entryId = group.GetDecimal(Tags.MDEntryID);
                var price = group.GetDecimal(Tags.MDEntryPx);
                var size = group.GetDecimal(Tags.MDEntrySize);
                var type = group.GetChar(Tags.MDEntryType);
                if (decimalPlaces == null) {
                    var priceString = group.GetString(Tags.MDEntryPx);
                    if (priceString.IndexOf(".") > 0)
                        decimalPlaces = priceString.Split('.')[1].Length;
                }
                var bookItem = new BookItem
                {
                    Price = price.ToDouble(),
                    Size = size.ToDouble(),
                    IsBid = type == '0',
                    EntryID = entryId.ToString(),
                    LocalTimeStamp = DateTime.Now,  
                    ServerTimeStamp = DateTime.Now,
                    DecimalPlaces = decimalPlaces.Value,
                    ProviderID = 12, //FXCM
                    Symbol = symbol,                    
                };

                switch (type)
                {
                    case '0':  // Bid
                        _bids.Add(bookItem);
                        break;
                    case '1':  // Ask
                        _asks.Add(bookItem);
                        break;
                }

            }

            var model = new OrderBook();
            model.LoadData(_asks, _bids);
            model.Symbol = symbol;
            model.DecimalPlaces = decimalPlaces.Value;
            model.SymbolMultiplier = Math.Pow(10, decimalPlaces.Value);
            model.ProviderID = 12; //FXCM
            model.ProviderName = "FXCM";

            // Raise an event or further process the data as needed
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType ="Market", ParsedModel = model, RawData = snapshot.ToString() });
        }
        private void HandleHeartBeat()
        {
            var provider = new VisualHFT.ViewModel.Model.Provider() { LastUpdated = DateTime.Now, ProviderCode=12, ProviderID=12, ProviderName="FXCM", Status = eSESSIONSTATUS.BOTH_CONNECTED };
            var model = new List<VisualHFT.ViewModel.Model.Provider>() { provider };
            // Raise an event or further process the data as needed
            OnDataReceived?.Invoke(this, new DataEventArgs { DataType = "HeartBeats", ParsedModel = model, RawData = "" });
        }

    }

}
