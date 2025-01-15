using VisualHFT.Commons.Model;

namespace VisualHFT.DataRetriever
{
    public interface IDataRetriever : IDisposable
    {
        Task StartAsync();
        Task StopAsync();
    }
    public class DataEventArgs : EventArgs, IResettable
    {
        public string DataType { get; set; }
        public string RawData { get; set; }
        public object ParsedModel { get; set; }

        public void Reset()
        {
            DataType = "";
            RawData = "";
            ParsedModel = null;
        }
    }
}