using System;

namespace VisualHFT.Model
{
    public class Execution
    {
        public Execution()
        {

        }
        public Execution(OpenExecution ex)
        {
            this.ExecutionID = ex.ExecutionID;
            this.PositionID = ex.PositionID;
            this.ClOrdId = ex.ClOrdId;
            this.ExecID = ex.ExecID;
            this.LocalTimeStamp = ex.LocalTimeStamp;
            this.ServerTimeStamp = ex.ServerTimeStamp;
            this.Price = ex.Price;
            this.ProviderID = ex.ProviderID;
            this.QtyFilled = ex.QtyFilled;
            this.Side = ex.Side;
            this.Status = ex.Status;
            this.IsOpen = ex.IsOpen;
        }
        public Execution(CloseExecution ex)
        {
            this.ExecutionID = ex.ExecutionID;
            this.PositionID = ex.PositionID;
            this.ClOrdId = ex.ClOrdId;
            this.ExecID = ex.ExecID;
            this.LocalTimeStamp = ex.LocalTimeStamp;
            this.ServerTimeStamp = ex.ServerTimeStamp;
            this.Price = ex.Price;
            this.ProviderID = ex.ProviderID;
            this.QtyFilled = ex.QtyFilled;
            this.Side = ex.Side;
            this.Status = ex.Status;
            this.IsOpen = ex.IsOpen;
        }
        public int ExecutionID { get; set; }
        public long PositionID { get; set; }
        public string ClOrdId { get; set; }
        public string ExecID { get; set; }
        public System.DateTime LocalTimeStamp { get; set; }
        public System.DateTime ServerTimeStamp { get; set; }
        public Nullable<decimal> Price { get; set; }
        public int ProviderID { get; set; }
        public Nullable<decimal> QtyFilled { get; set; }
        public Nullable<int> Side { get; set; }
        public Nullable<int> Status { get; set; }
        public bool IsOpen { get; set; }
    }
}
