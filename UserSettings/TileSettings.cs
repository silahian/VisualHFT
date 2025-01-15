using VisualHFT.Model;
using VisualHFT.Enums;

namespace VisualHFT.UserSettings
{
    public class TileSettings : ISetting
    {
        public string Symbol { get; set; }
        public Provider Provider { get; set; }
        public AggregationLevel AggregationLevel { get; set; }
    }
}