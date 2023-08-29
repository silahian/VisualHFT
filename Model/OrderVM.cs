using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VisualHFT.Model
{
    public class OrderVM : BindableBase
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
        private List<ExecutionVM> _executions;
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

        public OrderVM()
        {
            IsEmpty = true;
        }
        public void Update(OrderVM order)
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


            LastUpdated = DateTime.Now;
        }

        public double PendingQuantity => Quantity - FilledQuantity;
        public string ProviderName
        {
            get => _providerName;
            set => SetProperty(ref _providerName, value);
        }
        public long OrderID
        {
            get => _orderID;
            set => SetProperty(ref _orderID, value);
        }
        public string StrategyCode
        {
            get => _strategyCode;
            set => SetProperty(ref _strategyCode, value);
        }
        public string Symbol
        {
            get => _symbol;
            set => SetProperty(ref _symbol, value);
        }
        public int ProviderId
        {
            get => _providerId;
            set => SetProperty(ref _providerId, value);
        }
        public string ClOrdId
        {
            get => _clOrdId;
            set => SetProperty(ref _clOrdId, value);
        }
        public eORDERSIDE Side
        {
            get => _side;
            set => SetProperty(ref _side, value);
        }
        public eORDERTYPE OrderType
        {
            get => _orderType;
            set => SetProperty(ref _orderType, value);
        }
        public eORDERTIMEINFORCE TimeInForce
        {
            get => _timeInForce;
            set => SetProperty(ref _timeInForce, value);
        }
        public eORDERSTATUS Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        public double Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
        public double MinQuantity
        {
            get => _minQuantity;
            set => SetProperty(ref _minQuantity, value);
        }
        public double FilledQuantity
        {
            get => _filledQuantity;
            set => SetProperty(ref _filledQuantity, value);
        }
        public double PricePlaced
        {
            get => _pricePlaced;
            set => SetProperty(ref _pricePlaced, value);
        }
        public string Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }
        public string FutSettDate
        {
            get => _futSettDate;
            set => SetProperty(ref _futSettDate, value);
        }
        public bool IsMM
        {
            get => _isMM;
            set => SetProperty(ref _isMM, value);
        }
        public bool IsEmpty
        {
            get => _isEmpty;
            set => SetProperty(ref _isEmpty, value);
        }
        public string LayerName
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value);
        }
        public int AttemptsToClose
        {
            get => _attemptsToClose;
            set => SetProperty(ref _attemptsToClose, value);
        }
        public int SymbolMultiplier
        {
            get => _symbolMultiplier;
            set => SetProperty(ref _symbolMultiplier, value);
        }
        public int SymbolDecimals
        {
            get => _symbolDecimals;
            set => SetProperty(ref _symbolDecimals, value);
        }
        public string FreeText
        {
            get => _freeText;
            set => SetProperty(ref _freeText, value);
        }
        public string OriginPartyID
        {
            get => _originPartyID;
            set => SetProperty(ref _originPartyID, value);
        }
        public List<ExecutionVM> Executions
        {
            get => _executions;
            set => SetProperty(ref _executions, value);
        }
        public int QuoteID
        {
            get => _quoteID;
            set => SetProperty(ref _quoteID, value);
        }
        public DateTime QuoteServerTimeStamp
        {
            get => _quoteServerTimeStamp;
            set => SetProperty(ref _quoteServerTimeStamp, value);
        }
        public DateTime QuoteLocalTimeStamp
        {
            get => _quoteLocalTimeStamp;
            set => SetProperty(ref _quoteLocalTimeStamp, value);
        }
        public DateTime CreationTimeStamp
        {
            get => _creationTimeStamp;
            set => SetProperty(ref _creationTimeStamp, value);
        }
        public DateTime LastUpdated
        {
            get => _lastUpdate;
            set => SetProperty(ref _lastUpdate, value);
        }
        public DateTime ExecutedTimeStamp
        {
            get => _executedTimeStamp;
            set => SetProperty(ref _executedTimeStamp, value);
        }
        public DateTime FireSignalTimestamp
        {
            get => _fireSignalTimestamp;
            set => SetProperty(ref _fireSignalTimestamp, value);
        }
        public double StopLoss
        {
            get => _stopLoss;
            set => SetProperty(ref _stopLoss, value);
        }
        public double TakeProfit
        {
            get => _takeProfit;
            set => SetProperty(ref _takeProfit, value);
        }
        public bool PipsTrail
        {
            get => _pipsTrail;
            set => SetProperty(ref _pipsTrail, value);
        }
        public double UnrealizedPnL
        {
            get => _unrealizedPnL;
            set => SetProperty(ref _unrealizedPnL, value);
        }
        public double MaxDrowdown
        {
            get => _maxDrowdown;
            set => SetProperty(ref _maxDrowdown, value);
        }
        public double BestBid
        {
            get => _bestBid;
            set => SetProperty(ref _bestBid, value);
        }
        public double BestAsk
        {
            get => _bestAsk;
            set => SetProperty(ref _bestAsk, value);
        }
        public double GetAvgPrice
        {
            get => _getAvgPrice;
            set => SetProperty(ref _getAvgPrice, value);
        }
        public double GetQuantity
        {
            get => _getQuantity;
            set => SetProperty(ref _getQuantity, value);
        }
        public double FilledPercentage
        {
            get => _filledPercentage;
            set => SetProperty(ref _filledPercentage, value);
        }
    }

}
