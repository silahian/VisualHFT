using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demoTradingCore.Models
{
    public class jsonBookItem
    {
        public int DecimalPlaces { get; set; }
        public long EntryID { get; set; }
        public bool IsBid { get; set; }
        public string LayerName { get; set; }
        public DateTime LocalTimeStamp { get; set; }
        public decimal Price { get; set; }
        public int ProviderID { get; set; }
        public DateTime ServerTimeStamp { get; set; }
        public decimal Size { get; set; }
        public string Symbol { get; set; }
    }
    public class jsonMarket
    {
        public List<jsonBookItem> Asks { get; set; }
        public List<jsonBookItem> Bids { get; set; }
        public int DecimalPlaces { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public int ProviderStatus { get; set; }
        public string Symbol { get; set; }
        public int SymbolMultiplier { get; set; }
    }

    public class jsonMarkets: Json_BaseData
    {
        JsonSerializerSettings jsonSettings;
        public jsonMarkets()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy.MM.dd-hh.mm.ss.ffffff";

            this.type = "Market";
        }
        public string data 
        { 
            get {return Newtonsoft.Json.JsonConvert.SerializeObject(dataObj, jsonSettings); } 
        }

        public List<jsonMarket> dataObj { get; set; }
    }
}
