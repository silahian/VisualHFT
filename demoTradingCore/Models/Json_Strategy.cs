using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace demoTradingCore.Models
{
    public class Json_Strategy
    {
        public string StrategyCode { get; set; }
    }


    public class jsonStrategies: Json_BaseData 
    {
        protected JsonSerializerSettings jsonSettings;
        protected string _data;
        protected List<Json_Strategy> _dataObj;

        public jsonStrategies()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy.MM.dd-hh.mm.ss.ffffff";
            this.type = "Strategies";
            
        }
        public string data
        {
            get { return _data; }
        }
        public List<Json_Strategy> dataObj
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
