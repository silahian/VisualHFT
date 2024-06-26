namespace VisualHFT.Commons.Studies
{
    public interface IMultiStudy : IDisposable
    {
        List<IStudy> Studies { get; set; }

        Task StartAsync();
        Task StopAsync();
        string TileTitle { get; set; }
        string TileToolTip { get; set; }
    }

}