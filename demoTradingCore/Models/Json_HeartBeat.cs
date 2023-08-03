using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace demoTradingCore.Models
{

    public class Json_HeartBeats : Json_BaseData 
    {
        protected JsonSerializerSettings jsonSettings;
        protected string _data;
        protected List<Json_HeartBeat> _dataObj;

        public Json_HeartBeats()
        {
            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy.MM.dd-hh.mm.ss.ffffff";
            this.type = "HeartBeats";
            
        }
        ~Json_HeartBeats()
        {
            jsonSettings = null;
        }

        public string data
        {
            get { return _data; }
        }
        public List<Json_HeartBeat> dataObj
        {
            get { return _dataObj; }
            set
            {
                _dataObj = value;
                _data = Newtonsoft.Json.JsonConvert.SerializeObject(_dataObj, jsonSettings);
            }
        }

    }
    public class Json_HeartBeat
    {
        public int ProviderID;
        public string ProviderName;
        public int Status;
    }

}
