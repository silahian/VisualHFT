﻿//using CryptoExchange.Net.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.UserSettings;

namespace MarketConnectors.Kraken.Model
{
    public class PlugInSettings : ISetting
    {
        public string ApiKey { get; set; }
        public string PrivateKey { get; set; }
        public List<string> Symbols { get; set; }
        public int DepthLevels { get; set; }
        public int UpdateIntervalMs { get; set; }

        public string Symbol { get; set; }
        public VisualHFT.Model.Provider Provider { get; set; }
        public AggregationLevel AggregationLevel { get; set; }
    }
}
