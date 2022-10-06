using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Helpers;

namespace VisualHFT.Model
{
    public class ExecutionVM: OpenExecution
    {

        public ExecutionVM(OpenExecution exec, string symbol)
        {
            if (exec == null)
                return;
            this.ClOrdId = exec.ClOrdId;
            this.ExecID = exec.ExecID;
            this.ExecutionID = exec.ExecutionID;
            this.IsOpen = exec.IsOpen;
            this.LocalTimeStamp = exec.LocalTimeStamp;
            this.PositionID = exec.PositionID;
            this.Price = exec.Price;
            this.ProviderID = exec.ProviderID;
            this.QtyFilled = exec.QtyFilled;
            this.ServerTimeStamp = exec.ServerTimeStamp;
            this.Side = (ePOSITIONSIDE)exec.Side;
            this.Status = (ePOSITIONSTATUS)exec.Status;
            this.Symbol = symbol;
        }
        public ExecutionVM(CloseExecution exec, string symbol)
        {
            if (exec == null)
                return;
            this.ClOrdId = exec.ClOrdId;
            this.ExecID = exec.ExecID;
            this.ExecutionID = exec.ExecutionID;
            this.IsOpen = exec.IsOpen;
            this.LocalTimeStamp = exec.LocalTimeStamp;
            this.PositionID = exec.PositionID;
            this.Price = exec.Price;
            this.ProviderID = exec.ProviderID;
            this.QtyFilled = exec.QtyFilled;
            this.ServerTimeStamp = exec.ServerTimeStamp;
            this.Side = (ePOSITIONSIDE)exec.Side;
            this.Status = (ePOSITIONSTATUS)exec.Status;
            this.Symbol = symbol;
        }
        public string ProviderName { get; set; }
        public string Symbol { get; set; }
        public double LatencyInMiliseconds
        {
            get { return this.LocalTimeStamp.Subtract(this.ServerTimeStamp).TotalMilliseconds; }
        }
        public new ePOSITIONSIDE Side
        {
            get { return base.Side == null ? ePOSITIONSIDE.None : (ePOSITIONSIDE)base.Side; }
            set { base.Side = (int)value; }
        }
        public new ePOSITIONSTATUS Status
        {
            get { return base.Status == null? ePOSITIONSTATUS.NONE: (ePOSITIONSTATUS)base.Status; }
            set { base.Status = (int)value; }
        }
    }
    public class PositionEx: Position
    {
        /*
            No need to implement base fields notification methods, since this class won't be dynamic.
            It will be static collections, where only adding new items to the collection is the only thing dynamic.
        */
        public PositionEx()
        {
        }
        public PositionEx(Position p)
        {
            this.CloseExecutions = p.CloseExecutions.Select(x => new ExecutionVM(x, p.Symbol)).ToList();
            this.OpenExecutions = p.OpenExecutions.Select(x => new ExecutionVM(x, p.Symbol)).ToList();


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
            this.CloseStatus = (ePOSITIONSTATUS)p.CloseStatus;
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
            this.OpenStatus = (ePOSITIONSTATUS)p.OpenStatus;
            this.OrderQuantity = p.OrderQuantity;
            this.PipsPnLInCurrency = p.PipsPnLInCurrency;
            this.PipsTrail = p.PipsTrail;
            this.PositionID = p.PositionID;
            this.Side = (ePOSITIONSIDE)p.Side;
            this.StopLoss = p.StopLoss;
            this.StrategyCode = p.StrategyCode;
            this.Symbol = p.Symbol;
            this.SymbolDecimals = p.SymbolDecimals;
            this.SymbolMultiplier = p.SymbolMultiplier;
            this.TakeProfit = p.TakeProfit;
            this.UnrealizedPnL = p.UnrealizedPnL;
        }
        ~PositionEx()
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

        public new List<ExecutionVM> OpenExecutions {get; set;}
        public new List<ExecutionVM> CloseExecutions { get; set; }

        public List<ExecutionVM> AllExecutions {
            get {
                var _ret = new List<ExecutionVM>();
                if (this.OpenExecutions != null && this.OpenExecutions.Any())
                    _ret.AddRange(this.OpenExecutions);
                
                if (this.CloseExecutions != null && this.CloseExecutions.Any())
                    _ret.AddRange(this.CloseExecutions);
                return _ret/*.OrderBy(x => x.ServerTimeStamp)*/.ToList();
            }
        }
        
        public new ePOSITIONSIDE Side
        {
            get { return (ePOSITIONSIDE)base.Side; }
            set { base.Side = (int)value; }
        }
        public new ePOSITIONSTATUS CloseStatus
        {
            get { return (ePOSITIONSTATUS)base.CloseStatus; }
            set { base.CloseStatus = (int)value; }
        }
        public new ePOSITIONSTATUS OpenStatus
        {
            get { return (ePOSITIONSTATUS)base.OpenStatus; }
            set { base.OpenStatus = (int)value; }
        }

    }
}
