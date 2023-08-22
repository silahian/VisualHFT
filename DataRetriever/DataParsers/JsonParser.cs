using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.DataRetriever.DataParsers
{
    public class JsonParser : IDataParser
    {
        public T Parse<T>(string rawData)
        {
            // Use a JSON library like Newtonsoft.Json to deserialize the data
            return JsonConvert.DeserializeObject<T>(rawData);
        }
        public T Parse<T>(string rawData, dynamic settings)
        {
            // Use a JSON library like Newtonsoft.Json to deserialize the data
            return JsonConvert.DeserializeObject<T>(rawData, settings);
        }
    }

}
