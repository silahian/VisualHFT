using System.Collections.Generic;
using VisualHFT.UserSettings;

namespace MarketConnectors.WebSocket.Model
{
    public class PlugInSettings : ISetting
    {
        public required string HostName { get; set; }
        public required int Port { get; set; }
        public int ProviderId { get; set; }
        public required string ProviderName { get; set; }
        public string Symbol { get; set; }
        public VisualHFT.Model.Provider Provider { get; set; }
        public AggregationLevel AggregationLevel { get; set; }
    }
}