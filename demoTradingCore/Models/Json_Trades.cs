using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace demoTradingCore.Models
{
    public class jsonTrade
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; } = decimal.Zero;
        public decimal Size { get; set; } = decimal.Zero;
        public DateTime Timestamp { get; set; }
        public bool IsBuy { get; set; }
        public string Flags { get; set; }
    }

    public class jsonTrades: Json_BaseData
    {
        protected JsonSerializerSettings jsonSettings;
        protected string _data;
        protected List<jsonTrade> _dataObj;

        public jsonTrades()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy.MM.dd-hh.mm.ss.ffffff";
            this.type = "Trades";
        }
        public string data 
        { 
            get { return _data; }
            //get {return Newtonsoft.Json.JsonConvert.SerializeObject(dataObj, jsonSettings); } 

        }

        public List<jsonTrade> dataObj { 
            get { return _dataObj; }
            set 
            { 
                _dataObj = value;
                _data = Newtonsoft.Json.JsonConvert.SerializeObject(_dataObj, jsonSettings);
            }
        }
    }
}
