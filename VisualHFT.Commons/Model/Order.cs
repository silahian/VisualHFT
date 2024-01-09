
namespace VisualHFT.Model
{
    public partial class Order
    {
        /// <summary>
        /// This override will fire PostedSecondsAgo property change when any other property fires
        /// </summary>
        /// <param name="args"></param>

        #region private fields
        private string _providerName;
        private long _orderID;
        private string _strategyCode;
        private string _symbol;
        private int _providerId;
        private string _clOrdId;
        private eORDERSIDE _side;
        private eORDERTYPE _orderType;
        private eORDERTIMEINFORCE _timeInForce;
        private eORDERSTATUS _status;
        private double _quantity;
        private double _minQuantity;
        private double _filledQuantity;
        private double _pricePlaced;
        private string _currency;
        private string _futSettDate;
        private bool _isMM;
        private bool _isEmpty;
        private string _layerName;
        private int _attemptsToClose;
        private int _symbolMultiplier;
        private int _symbolDecimals;
        private string _freeText;
        private string _originPartyID;
        private List<Execution> _executions;
        private int _quoteID;
        private DateTime _quoteServerTimeStamp;
        private DateTime _quoteLocalTimeStamp;
        private DateTime _creationTimeStamp;
        private DateTime _lastUpdate;
        private DateTime _executedTimeStamp;
        private DateTime _fireSignalTimestamp;
        private double _stopLoss;
        private double _takeProfit;
        private bool _pipsTrail;
        private double _unrealizedPnL;
        private double _maxDrowdown;
        private double _bestBid;
        private double _bestAsk;
        private double _getAvgPrice;
        private double _getQuantity;
        private double _filledPercentage;
        #endregion

        public Order()
        {
            IsEmpty = true;
        }
        public void Update(Order order)
        {
            ProviderName = order.ProviderName;
            OrderID = order.OrderID;
            StrategyCode = order.StrategyCode;
            Symbol = order.Symbol;
            ProviderId = order.ProviderId;
            ClOrdId = order.ClOrdId;
            Side = order.Side;
            OrderType = order.OrderType;
            TimeInForce = order.TimeInForce;
            Status = order.Status;
            Quantity = order.Quantity;
            MinQuantity = order.MinQuantity;
            FilledQuantity = order.FilledQuantity;
            PricePlaced = order.PricePlaced;
            Currency = order.Currency;
            FutSettDate = order.FutSettDate;
            IsMM = order.IsMM;
            IsEmpty = order.IsEmpty;
            LayerName = order.LayerName;
            AttemptsToClose = order.AttemptsToClose;
            SymbolMultiplier = order.SymbolMultiplier;
            SymbolDecimals = order.SymbolDecimals;
            FreeText = order.FreeText;
            OriginPartyID = order.OriginPartyID;
            Executions = order.Executions;
            QuoteID = order.QuoteID;
            QuoteServerTimeStamp = order.QuoteServerTimeStamp;
            QuoteLocalTimeStamp = order.QuoteLocalTimeStamp;
            CreationTimeStamp = order.CreationTimeStamp;
            FireSignalTimestamp = order.FireSignalTimestamp;
            StopLoss = order.StopLoss;
            TakeProfit = order.TakeProfit;
            PipsTrail = order.PipsTrail;
            UnrealizedPnL = order.UnrealizedPnL;
            MaxDrowdown = order.MaxDrowdown;
            BestAsk = order.BestAsk;
            BestBid = order.BestBid;
            GetAvgPrice = order.GetAvgPrice;
            GetQuantity = order.GetQuantity;


            LastUpdated = HelperTimeProvider.Now;
        }

        public double PendingQuantity => Quantity - FilledQuantity;
        public string ProviderName
        {
            get => _providerName;
            set => _providerName = value;
        }
        public long OrderID
        {
            get => _orderID;
            set => _orderID = value;
        }
        public string StrategyCode
        {
            get => _strategyCode;
            set => _strategyCode = value;
        }
        public string Symbol
        {
            get => _symbol;
            set => _symbol = value;
        }
        public int ProviderId
        {
            get => _providerId;
            set => _providerId = value;
        }
        public string ClOrdId
        {
            get => _clOrdId;
            set => _clOrdId = value;
        }
        public eORDERSIDE Side
        {
            get => _side;
            set => _side = value;
        }
        public eORDERTYPE OrderType
        {
            get => _orderType;
            set => _orderType = value;
        }
        public eORDERTIMEINFORCE TimeInForce
        {
            get => _timeInForce;
            set => _timeInForce = value;
        }
        public eORDERSTATUS Status
        {
            get => _status;
            set => _status = value;
        }
        public double Quantity
        {
            get => _quantity;
            set => _quantity = value;
        }
        public double MinQuantity
        {
            get => _minQuantity;
            set => _minQuantity = value;
        }
        public double FilledQuantity
        {
            get => _filledQuantity;
            set => _filledQuantity = value;
        }
        public double PricePlaced
        {
            get => _pricePlaced;
            set => _pricePlaced = value;
        }
        public string Currency
        {
            get => _currency;
            set => _currency = value;
        }
        public string FutSettDate
        {
            get => _futSettDate;
            set => _futSettDate = value;
        }
        public bool IsMM
        {
            get => _isMM;
            set => _isMM = value;
        }
        public bool IsEmpty
        {
            get => _isEmpty;
            set => _isEmpty = value;
        }
        public string LayerName
        {
            get => _layerName;
            set => _layerName = value;
        }
        public int AttemptsToClose
        {
            get => _attemptsToClose;
            set => _attemptsToClose = value ;
        }
        public int SymbolMultiplier
        {
            get => _symbolMultiplier;
            set => _symbolMultiplier = value;
        }
        public int SymbolDecimals
        {
            get => _symbolDecimals;
            set => _symbolDecimals = value;
        }
        public string FreeText
        {
            get => _freeText;
            set => _freeText = value;
        }
        public string OriginPartyID
        {
            get => _originPartyID;
            set => _originPartyID = value;
        }
        public List<Execution> Executions
        {
            get => _executions;
            set => _executions = value;
        }
        public int QuoteID
        {
            get => _quoteID;
            set => _quoteID = value;
        }
        public DateTime QuoteServerTimeStamp
        {
            get => _quoteServerTimeStamp;
            set => _quoteServerTimeStamp = value;
        }
        public DateTime QuoteLocalTimeStamp
        {
            get => _quoteLocalTimeStamp;
            set => _quoteLocalTimeStamp = value;
        }
        public DateTime CreationTimeStamp
        {
            get => _creationTimeStamp;
            set => _creationTimeStamp = value;
        }
        public DateTime LastUpdated
        {
            get => _lastUpdate;
            set => _lastUpdate = value;
        }
        public DateTime ExecutedTimeStamp
        {
            get => _executedTimeStamp;
            set => _executedTimeStamp = value;
        }
        public DateTime FireSignalTimestamp
        {
            get => _fireSignalTimestamp;
            set => _fireSignalTimestamp = value;
        }
        public double StopLoss
        {
            get => _stopLoss;
            set => _stopLoss = value;
        }
        public double TakeProfit
        {
            get => _takeProfit;
            set => _takeProfit = value;
        }
        public bool PipsTrail
        {
            get => _pipsTrail;
            set => _pipsTrail = value;
        }
        public double UnrealizedPnL
        {
            get => _unrealizedPnL;
            set => _unrealizedPnL = value;
        }
        public double MaxDrowdown
        {
            get => _maxDrowdown;
            set => _maxDrowdown = value;
        }
        public double BestBid
        {
            get => _bestBid;
            set => _bestBid = value;
        }
        public double BestAsk
        {
            get => _bestAsk;
            set => _bestAsk = value;
        }
        public double GetAvgPrice
        {
            get => _getAvgPrice;
            set => _getAvgPrice = value;
        }
        public double GetQuantity
        {
            get => _getQuantity;
            set => _getQuantity = value;
        }
        public double FilledPercentage
        {
            get => _filledPercentage;
            set => _filledPercentage = value;
        }
    }

}
