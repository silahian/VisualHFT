using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public partial class CloseExecution
    {
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
