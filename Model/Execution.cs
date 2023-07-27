using System;

namespace VisualHFT.Model
{
    public class Execution
    {
        public Execution(OpenExecution execution)
        {
            ExecutionID = execution.ExecutionID;
            PositionID = execution.PositionID;
            ClOrdId = execution.ClOrdId;
            ExecID = execution.ExecID;
            LocalTimeStamp = execution.LocalTimeStamp;
            ServerTimeStamp = execution.ServerTimeStamp;
            Price = execution.Price;
            ProviderID = execution.ProviderID;
            QtyFilled = execution.QtyFilled;
            Side = execution.Side;
            Status = execution.Status;
            IsOpen = execution.IsOpen;
        }
        public Execution(CloseExecution execution)
        {
            ExecutionID = execution.ExecutionID;
            PositionID = execution.PositionID;
            ClOrdId = execution.ClOrdId;
            ExecID = execution.ExecID;
            LocalTimeStamp = execution.LocalTimeStamp;
            ServerTimeStamp = execution.ServerTimeStamp;
            Price = execution.Price;
            ProviderID = execution.ProviderID;
            QtyFilled = execution.QtyFilled;
            Side = execution.Side;
            Status = execution.Status;
            IsOpen = execution.IsOpen;
        }
        public int ExecutionID { get; set; }
        public long PositionID { get; set; }
        public string ClOrdId { get; set; }
        public string ExecID { get; set; }
        public DateTime LocalTimeStamp { get; set; }
        public DateTime ServerTimeStamp { get; set; }
        public decimal? Price { get; set; }
        public int ProviderID { get; set; }
        public decimal? QtyFilled { get; set; }
        public int? Side { get; set; }
        public int? Status { get; set; }
        public bool IsOpen { get; set; }
    }
}
