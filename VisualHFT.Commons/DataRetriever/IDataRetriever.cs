using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.DataRetriever
{
    public interface IDataRetriever: IDisposable
    {
        event EventHandler<DataEventArgs> OnDataReceived;
        void Start();
        void Stop();
    }
    public class DataEventArgs : EventArgs
    {
        public string DataType { get; set; }
        public string RawData { get; set; }
        public object ParsedModel { get; set; }
    }
}
