using System.Collections.Generic;
using VisualHFT.Model;
using VisualHFT.UserSettings;

namespace VisualHFT.Studies.LOBImbalance.Model
{
    public class PlugInSettings : ISetting
    {
        public string Symbol { get; set; }
        public Provider Provider { get; set; }
        public AggregationLevel AggregationLevel { get; set; }
    }
}
