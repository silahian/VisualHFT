using System.Collections.Generic;
using VisualHFT.Enums;
using VisualHFT.UserSettings;

namespace MarketConnectors.Binance.Model
{
    public class PlugInSettings : ISetting
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public List<string> Symbols { get; set; }
        public int DepthLevels { get; set; }
        public int UpdateIntervalMs { get; set; }
        public bool IsNonUS { get; set; }

        public string Symbol { get; set; }
        public VisualHFT.Model.Provider Provider { get; set; }
        public AggregationLevel AggregationLevel { get; set; }
    }
}