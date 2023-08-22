using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.DataRetriever.DataParsers
{
    public interface IDataParser
    {
        T Parse<T>(string rawData);
        T Parse<T>(string rawData, dynamic settings);
    }

}
