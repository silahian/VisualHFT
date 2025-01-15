using VisualHFT.Commons.Model;
using VisualHFT.Enums;

namespace VisualHFT.Model
{
    public partial class Execution : OpenExecution, IResettable
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
            if (exec.Side != null)
                this.Side = (ePOSITIONSIDE)exec.Side;
            if (exec.Status != null)
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
            if (exec.Side != null)
                this.Side = (ePOSITIONSIDE)exec.Side;
            if (exec.Status != null)
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

        public void Reset()
        {
            this.ClOrdId = "";
            this.ExecID = "";
            this.ExecutionID = 0;
            this.IsOpen = true;
            this.LocalTimeStamp = DateTime.MinValue;
            this.PositionID = 0;
            this.Price = 0;
            this.ProviderID = 0;
            this.QtyFilled = 0;
            this.ServerTimeStamp = DateTime.MinValue;
            this.Side = ePOSITIONSIDE.None;
            this.Status = ePOSITIONSTATUS.NONE;
            this.Symbol = "";
        }
    }
}
