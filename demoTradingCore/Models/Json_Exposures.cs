using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demoTradingCore.Models
{
    public class Json_Exposure
    {
        public decimal SizeExposed { get; set; }
        public string StrategyName { get; set; }
        public string Symbol { get; set; }
        public decimal UnrealizedPL { get; set; }
    }

    public class JsonExposures : Json_BaseData
    {
        protected JsonSerializerSettings jsonSettings;
        protected string _data;
        protected List<Json_Exposure> _dataObj;

        public JsonExposures()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy.MM.dd-hh.mm.ss.ffffff";
            this.type = "Exposures";

        }
        public string data
        {
            get { return _data; }
        }
        public List<Json_Exposure> dataObj
        {
            get { return _dataObj; }
            set
            {
                _dataObj = value;
                _data = Newtonsoft.Json.JsonConvert.SerializeObject(_dataObj, jsonSettings);
            }
        }
    }
}
