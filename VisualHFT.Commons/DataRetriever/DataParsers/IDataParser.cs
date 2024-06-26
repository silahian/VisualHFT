namespace VisualHFT.DataRetriever.DataParsers
{
    public interface IDataParser
    {
        T Parse<T>(string rawData);
        T Parse<T>(string rawData, dynamic settings);
    }

}