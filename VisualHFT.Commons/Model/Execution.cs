using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public partial class Execution : OpenExecution
    {

        public Execution(OpenExecution exec, string symbol)
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
        public Execution(CloseExecution exec, string symbol)
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
            get { return base.Status == null ? ePOSITIONSTATUS.NONE : (ePOSITIONSTATUS)base.Status; }
            set { base.Status = (int)value; }
        }

        public string OrigClOrdID { get; set; }
    }
}
