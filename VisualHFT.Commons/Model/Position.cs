namespace VisualHFT.Model
{
    using System;
    using System.Collections.Generic;
    using VisualHFT.Enums;

    public partial class Position
    {
        public Position()
        {
            this.CloseExecutions = new List<Execution>();
            this.OpenExecutions = new List<Execution>();
        }
        public Position(Position p)
        {
            this.CloseExecutions = p.CloseExecutions.Select(x => new Execution(x, p.Symbol)).ToList();
            this.OpenExecutions = p.OpenExecutions.Select(x => new Execution(x, p.Symbol)).ToList();


            this.AttemptsToClose = p.AttemptsToClose;
            this.CloseBestAsk = p.CloseBestAsk;
            this.CloseBestBid = p.CloseBestBid;
            this.CloseClOrdId = p.CloseClOrdId;
            this.CloseFireSignalTimestamp = p.CloseFireSignalTimestamp;
            this.CloseOriginPartyID = p.CloseOriginPartyID;
            this.CloseProviderId = p.CloseProviderId;
            this.CloseQuoteId = p.CloseQuoteId;
            this.CloseQuoteLocalTimeStamp = p.CloseQuoteLocalTimeStamp;
            this.CloseQuoteServerTimeStamp = p.CloseQuoteServerTimeStamp;
            this.CloseStatus = p.CloseStatus;
            this.CloseTimeStamp = p.CloseTimeStamp;
            this.CreationTimeStamp = p.CreationTimeStamp;
            this.Currency = p.Currency;
            this.FreeText = p.FreeText;
            this.FutSettDate = p.FutSettDate;
            this.GetCloseAvgPrice = p.GetCloseAvgPrice;
            this.GetCloseQuantity = p.GetCloseQuantity;
            this.GetOpenAvgPrice = p.GetOpenAvgPrice;
            this.GetOpenQuantity = p.GetOpenQuantity;
            this.GetPipsPnL = p.GetPipsPnL;
            this.ID = p.ID;
            this.IsCloseMM = p.IsCloseMM;
            this.IsOpenMM = p.IsOpenMM;
            this.LayerName = this.LayerName;
            this.MaxDrowdown = p.MaxDrowdown;
            this.OpenBestAsk = p.OpenBestAsk;
            this.OpenBestBid = p.OpenBestBid;
            this.OpenClOrdId = p.OpenClOrdId;
            this.OpenFireSignalTimestamp = p.OpenFireSignalTimestamp;
            this.OpenOriginPartyID = p.OpenOriginPartyID;
            this.OpenProviderId = p.OpenProviderId;
            this.OpenQuoteId = p.OpenQuoteId;
            this.OpenQuoteLocalTimeStamp = p.OpenQuoteLocalTimeStamp;
            this.OpenQuoteServerTimeStamp = p.OpenQuoteServerTimeStamp;
            this.OpenStatus = p.OpenStatus;
            this.OrderQuantity = p.OrderQuantity;
            this.PipsPnLInCurrency = p.PipsPnLInCurrency;
            this.PipsTrail = p.PipsTrail;
            this.PositionID = p.PositionID;
            this.Side = p.Side;
            this.StopLoss = p.StopLoss;
            this.StrategyCode = p.StrategyCode;
            this.Symbol = p.Symbol;
            this.SymbolDecimals = p.SymbolDecimals;
            this.SymbolMultiplier = p.SymbolMultiplier;
            this.TakeProfit = p.TakeProfit;
            this.UnrealizedPnL = p.UnrealizedPnL;
        }
        ~Position()
        {
            if (this.CloseExecutions != null)
                this.CloseExecutions.Clear();
            this.CloseExecutions = null;

            if (this.OpenExecutions != null)
                this.OpenExecutions.Clear();
            this.OpenExecutions = null;
        }

        public string OpenProviderName { get; set; }
        public string CloseProviderName { get; set; }
        public List<Execution> AllExecutions
        {
            get
            {
                var _ret = new List<Execution>();
                if (this.OpenExecutions != null && this.OpenExecutions.Any())
                    _ret.AddRange(this.OpenExecutions);

                if (this.CloseExecutions != null && this.CloseExecutions.Any())
                    _ret.AddRange(this.CloseExecutions);
                return _ret/*.OrderBy(x => x.ServerTimeStamp)*/.ToList();
            }
        }


        public long ID { get; set; }
        public long PositionID { get; set; }
        public int AttemptsToClose { get; set; }
        public string CloseClOrdId { get; set; }
        public int CloseProviderId { get; set; }
        public Nullable<int> CloseQuoteId { get; set; }
        public Nullable<System.DateTime> CloseQuoteLocalTimeStamp { get; set; }
        public Nullable<System.DateTime> CloseQuoteServerTimeStamp { get; set; }
        public int CloseStatus { get; set; }
        public System.DateTime CloseTimeStamp { get; set; }
        public System.DateTime CreationTimeStamp { get; set; }
        public string Currency { get; set; }
        public string FreeText { get; set; }
        public Nullable<System.DateTime> FutSettDate { get; set; }
        public decimal GetCloseAvgPrice { get; set; }
        public decimal GetCloseQuantity { get; set; }
        public decimal GetOpenAvgPrice { get; set; }
        public decimal GetOpenQuantity { get; set; }
        public decimal GetPipsPnL { get; set; }
        public bool IsCloseMM { get; set; }
        public bool IsOpenMM { get; set; }
        public decimal MaxDrowdown { get; set; }
        public string OpenClOrdId { get; set; }
        public int OpenProviderId { get; set; }
        public Nullable<int> OpenQuoteId { get; set; }
        public Nullable<System.DateTime> OpenQuoteLocalTimeStamp { get; set; }
        public Nullable<System.DateTime> OpenQuoteServerTimeStamp { get; set; }
        public int OpenStatus { get; set; }
        public decimal OrderQuantity { get; set; }
        public decimal PipsTrail { get; set; }
        public ePOSITIONSIDE Side { get; set; }
        public decimal StopLoss { get; set; }
        public string StrategyCode { get; set; }
        public string Symbol { get; set; }
        public int SymbolDecimals { get; set; }
        public int SymbolMultiplier { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public Nullable<decimal> OpenBestBid { get; set; }
        public Nullable<decimal> OpenBestAsk { get; set; }
        public Nullable<decimal> CloseBestBid { get; set; }
        public Nullable<decimal> CloseBestAsk { get; set; }
        public string OpenOriginPartyID { get; set; }
        public string CloseOriginPartyID { get; set; }
        public string LayerName { get; set; }
        public Nullable<System.DateTime> OpenFireSignalTimestamp { get; set; }
        public Nullable<System.DateTime> CloseFireSignalTimestamp { get; set; }
        public Nullable<decimal> PipsPnLInCurrency { get; set; }

        public virtual List<Execution> CloseExecutions { get; set; }
        public virtual List<Execution> OpenExecutions { get; set; }
        private Order GetOrder(bool isOpen)
        {
            if (!string.IsNullOrEmpty(this.OpenClOrdId))
            {
                Order o = new Order();
                //o.OrderID
                o.Currency = this.Currency;
                o.ClOrdId = isOpen ? this.OpenClOrdId : this.CloseClOrdId;
                o.ProviderId = isOpen ? this.OpenProviderId : this.CloseProviderId;
                o.ProviderName = isOpen ? this.OpenProviderName : this.CloseProviderName;
                o.LayerName = this.LayerName;
                o.AttemptsToClose = this.AttemptsToClose;
                o.BestAsk = isOpen ? this.OpenBestAsk.ToDouble() : this.CloseBestAsk.ToDouble();
                o.BestBid = isOpen ? this.OpenBestBid.ToDouble() : this.CloseBestBid.ToDouble();
                o.CreationTimeStamp = isOpen ? this.OpenQuoteLocalTimeStamp.ToDateTime() : this.CloseQuoteLocalTimeStamp.ToDateTime();
                o.Executions = isOpen ? this.OpenExecutions.ToList() : this.CloseExecutions.ToList();
                o.SymbolMultiplier = this.SymbolMultiplier;
                o.Symbol = this.Symbol;
                o.FreeText = this.FreeText;
                o.Status = (eORDERSTATUS)(isOpen ? this.OpenStatus : this.CloseStatus);
                o.GetAvgPrice = isOpen ? this.GetOpenAvgPrice.ToDouble() : this.GetCloseAvgPrice.ToDouble();

                o.GetQuantity = isOpen ? this.GetOpenQuantity.ToDouble() : this.GetCloseQuantity.ToDouble();
                o.Quantity = this.OrderQuantity.ToDouble();
                o.FilledQuantity = isOpen ? this.GetOpenQuantity.ToDouble() : this.GetCloseQuantity.ToDouble();

                o.IsEmpty = false;
                o.IsMM = isOpen ? this.IsOpenMM : this.IsCloseMM;
                //o.MaxDrowdown = 
                //o.MinQuantity = 
                //o.OrderID = 

                //TO-DO: we need to find a way to add this.
                //*************o.OrderType = this 

                //o.PipsTrail

                o.PricePlaced = o.Executions.Where(x => x.Status == ePOSITIONSTATUS.SENT || x.Status == ePOSITIONSTATUS.NEW || x.Status == ePOSITIONSTATUS.REPLACESENT)
                    .First().Price.ToDouble();
                if (o.PricePlaced == 0) //if this happens, is because the data is corrupted. But, in order to auto-fix it, we use AvgPrice
                {
                    o.PricePlaced = o.GetAvgPrice;
                }
                o.QuoteID = isOpen ? this.OpenQuoteId.ToInt() : this.CloseQuoteId.ToInt();
                o.QuoteLocalTimeStamp = isOpen ? this.OpenQuoteLocalTimeStamp.ToDateTime() : this.CloseQuoteLocalTimeStamp.ToDateTime();
                o.QuoteServerTimeStamp = isOpen ? this.OpenQuoteServerTimeStamp.ToDateTime() : this.CloseQuoteServerTimeStamp.ToDateTime();
                if (isOpen)
                    o.Side = (eORDERSIDE)this.Side;
                else
                    o.Side = (eORDERSIDE)(this.Side == ePOSITIONSIDE.Sell ? eORDERSIDE.Buy : eORDERSIDE.Sell); //the opposite
                //o.StopLoss = 
                o.StrategyCode = this.StrategyCode;
                o.SymbolDecimals = this.SymbolDecimals;
                o.SymbolMultiplier = this.SymbolMultiplier;
                //o.TakeProfit

                //TO-DO: we need to find a way to add this.
                //*************o.TimeInForce = 

                //o.UnrealizedPnL               
                o.LastUpdated = HelperTimeProvider.Now;
                o.FilledPercentage = 100 * (o.FilledQuantity / o.Quantity);
                return o;
            }
            return null;
        }
        public List<Order> GetOrders()
        {

            Order openOrder = GetOrder(true);
            Order closeOrder = GetOrder(false);
            var orders = new List<Order>();
            if (openOrder != null)
                orders.Add(openOrder);
            if (closeOrder != null)
                orders.Add(closeOrder);


            return orders;
        }


    }
}
