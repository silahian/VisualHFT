using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace VisualHFT.Model
{
    public class OrderVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            OnPropertyChanged("PostedSecondsAgo");
            return true;
        }
        #region private fields
        protected string _ProviderName;
        protected long _OrderID;
        protected string _StrategyCode;
        protected string _Symbol ;
        protected int _ProviderId ;
        protected string _ClOrdId ;
        protected eORDERSIDE _Side ;
        protected eORDERTYPE _OrderType ;
        protected eORDERTIMEINFORCE _TimeInForce ;
        protected eORDERSTATUS _Status ;
        protected double _Quantity ;
        protected double _MinQuantity ;
        protected double _FilledQuantity ;
        protected double _PricePlaced ;
        protected string _Currency ;
        protected string _FutSettDate ;
        protected bool _IsMM ;
        protected bool _IsEmpty ;
        protected string _LayerName ;
        protected int _AttemptsToClose ;
        protected int _SymbolMultiplier ;
        protected int _SymbolDecimals ;
        protected string _FreeText ;
        protected string _OriginPartyID ;
        protected IEnumerable<OpenExecution> _Executions;
        protected int _QuoteID ;
        protected DateTime _QuoteServerTimeStamp ;
        protected DateTime _QuoteLocalTimeStamp ;
        protected DateTime _CreationTimeStamp ;
        protected DateTime _ExecutedTimeStamp ;
        protected DateTime _FireSignalTimestamp ;
        protected double _StopLoss ;
        protected double _TakeProfit ;
        protected bool _PipsTrail ;
        protected double _UnrealizedPnL ;
        protected double _MaxDrowdown ;
        protected double _BestBid ;
        protected double _BestAsk ;
        protected double _GetAvgPrice;
        protected double _GetQuantity ;
        #endregion

        public OrderVM()
        {
            this.IsEmpty = true;
        }
        public void Update(OrderVM o)
        {
            this.ProviderName = o.ProviderName;
            this.OrderID = o.OrderID;
            this.StrategyCode = o.StrategyCode;
            this.Symbol = o.Symbol;
            this.ProviderId = o.ProviderId;
            this.ClOrdId = o.ClOrdId;
            this.Side = o.Side;
            this.OrderType = o.OrderType;
            this.TimeInForce = o.TimeInForce;
            this.Status = o.Status;
            this.Quantity = o.Quantity; 
            this.MinQuantity = o.MinQuantity;
            this.FilledQuantity = o.FilledQuantity;
            this.PricePlaced = o.PricePlaced; 
            this.Currency = o.Currency;
            this.FutSettDate = o.FutSettDate;
            this.IsMM = o.IsMM;
            this.IsEmpty = o.IsEmpty;
            this.LayerName = o.LayerName;
            this.AttemptsToClose = o.AttemptsToClose;
            this.SymbolMultiplier = o.SymbolMultiplier;
            this.SymbolDecimals = o.SymbolDecimals;
            this.FreeText = o.FreeText; 
            this.OriginPartyID = o.OriginPartyID;
            this.Executions = o.Executions;
            this.QuoteID = o.QuoteID;
            this.QuoteServerTimeStamp = o.QuoteServerTimeStamp;
            this.QuoteLocalTimeStamp = o.QuoteLocalTimeStamp;
            this.CreationTimeStamp = o.CreationTimeStamp;
            this.FireSignalTimestamp = o.FireSignalTimestamp;
            this.StopLoss = o.StopLoss;
            this.TakeProfit = o.TakeProfit;
            this.PipsTrail = o.PipsTrail; 
            this.UnrealizedPnL = o.UnrealizedPnL;
            this.MaxDrowdown = o.MaxDrowdown;
            this.BestAsk = o.BestAsk;
            this.BestBid = o.BestBid;
            this.GetAvgPrice = o.GetAvgPrice;
            this.GetQuantity = o.GetQuantity;
            
        }
        public double PostedSecondsAgo { get { return DateTime.Now.Subtract(this.CreationTimeStamp).TotalSeconds; } }
        public double PendingQuantity { get { return _Quantity - _FilledQuantity; } }


        public string ProviderName { get { return _ProviderName; } set { SetField(ref _ProviderName, value, "ProviderName"); } }
        public long OrderID{ get { return _OrderID; } set { SetField(ref _OrderID, value, "OrderID"); } }
        public string StrategyCode{ get { return _StrategyCode; } set { SetField(ref _StrategyCode, value, "StrategyCode"); } }
        public string Symbol{ get { return _Symbol; } set { SetField(ref _Symbol, value, "Symbol"); } }
        public int ProviderId{ get { return _ProviderId; } set { SetField(ref _ProviderId, value, "ProviderId"); } }
        public string ClOrdId{ get { return _ClOrdId; } set { SetField(ref _ClOrdId, value, "ClOrdId"); } }
        public eORDERSIDE Side { get { return _Side; } set { SetField(ref _Side, value, "Side"); } }
        public eORDERTYPE OrderType { get { return _OrderType; } set { SetField(ref _OrderType, value, "OrderType"); } }
        public eORDERTIMEINFORCE TimeInForce { get { return _TimeInForce; } set { SetField(ref _TimeInForce, value, "TimeInForce"); } }
        public eORDERSTATUS Status { get { return _Status; } set { SetField(ref _Status, value, "Status"); } }
        public double Quantity{ get { return _Quantity; } set { SetField(ref _Quantity, value, "Quantity"); } }
        public double MinQuantity{ get { return _MinQuantity; } set { SetField(ref _MinQuantity, value, "MinQuantity"); } }
        public double FilledQuantity{ get { return _FilledQuantity; } set { SetField(ref _FilledQuantity, value, "FilledQuantity"); OnPropertyChanged("PendingQuantity"); } }
        public double PricePlaced{ get { return _PricePlaced; } set { SetField(ref _PricePlaced, value, "PricePlaced"); } }
        public string Currency{ get { return _Currency; } set { SetField(ref _Currency, value, "Currency"); } }
        public string FutSettDate{ get { return _FutSettDate; } set { SetField(ref _FutSettDate, value, "FutSettDate"); } }
        public bool IsMM{ get { return _IsMM; } set { SetField(ref _IsMM, value, "IsMM"); } }
        public bool IsEmpty{ get { return _IsEmpty; } set { SetField(ref _IsEmpty, value, "IsEmpty"); } }
        public string LayerName{ get { return _LayerName; } set { SetField(ref _LayerName, value, "LayerName"); } }
        public int AttemptsToClose{ get { return _AttemptsToClose; } set { SetField(ref _AttemptsToClose, value, "AttemptsToClose"); } }
        public int SymbolMultiplier{ get { return _SymbolMultiplier; } set { SetField(ref _SymbolMultiplier, value, "SymbolMultiplier"); } }
        public int SymbolDecimals{ get { return _SymbolDecimals; } set { SetField(ref _SymbolDecimals, value, "SymbolDecimals"); } }
        public string FreeText{ get { return _FreeText; } set { SetField(ref _FreeText, value, "FreeText"); } }
        public string OriginPartyID{ get { return _OriginPartyID; } set { SetField(ref _OriginPartyID, value, "OriginPartyID"); } }
        public IEnumerable<OpenExecution> Executions{ get { return _Executions; } set { SetField(ref _Executions, value, "Executions"); } }
        public int QuoteID { get { return _QuoteID; } set { SetField(ref _QuoteID, value, "QuoteID"); } }
        public DateTime QuoteServerTimeStamp{ get { return _QuoteServerTimeStamp; } set { SetField(ref _QuoteServerTimeStamp, value, "QuoteServerTimeStamp"); } }
        public DateTime QuoteLocalTimeStamp{ get { return _QuoteLocalTimeStamp; } set { SetField(ref _QuoteLocalTimeStamp, value, "QuoteLocalTimeStamp"); } }
        public DateTime CreationTimeStamp{ get { return _CreationTimeStamp; } set { SetField(ref _CreationTimeStamp, value, "CreationTimeStamp"); OnPropertyChanged("PostedSecondsAgo"); } } 
        public DateTime ExecutedTimeStamp{ get { return _ExecutedTimeStamp; } set { SetField(ref _ExecutedTimeStamp, value, "ExecutedTimeStamp"); } }
        public DateTime FireSignalTimestamp{ get { return _FireSignalTimestamp; } set { SetField(ref _FireSignalTimestamp, value, "FireSignalTimestamp"); } }
        public double StopLoss{ get { return _StopLoss; } set { SetField(ref _StopLoss, value, "StopLoss"); } }
        public double TakeProfit{ get { return _TakeProfit; } set { SetField(ref _TakeProfit, value, "TakeProfit"); } }
        public bool PipsTrail{ get { return _PipsTrail; } set { SetField(ref _PipsTrail, value, "PipsTrail"); } }
        public double UnrealizedPnL{ get { return _UnrealizedPnL; } set { SetField(ref _UnrealizedPnL, value, "UnrealizedPnL"); } }
        public double MaxDrowdown{ get { return _MaxDrowdown; } set { SetField(ref _MaxDrowdown, value, "MaxDrowdown"); } }
        public double BestBid{ get { return _BestBid; } set { SetField(ref _BestBid, value, "BestBid"); } }
        public double BestAsk{ get { return _BestAsk; } set { SetField(ref _BestAsk, value, "BestAsk"); } }
        public double GetAvgPrice { get { return _GetAvgPrice; } set { SetField(ref _GetAvgPrice, value, "GetAvgPrice"); } }
        public double GetQuantity { get { return _GetQuantity; } set { SetField(ref _GetQuantity, value, "GetQuantity"); } }
    }
}
