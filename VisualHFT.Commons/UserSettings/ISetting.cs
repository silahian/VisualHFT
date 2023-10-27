using VisualHFT.Model;

namespace VisualHFT.UserSettings
{
    public interface ISetting
    {
        string Symbol { get; set; }
        Provider Provider { get; set; }
        AggregationLevel AggregationLevel { get; set; }
    }

}
