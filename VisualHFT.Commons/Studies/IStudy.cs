using VisualHFT.Model;

namespace VisualHFT.Commons.Studies
{
    public interface IStudy : IDisposable
    {
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;

        Task StartAsync();
        Task StopAsync();
        string TileTitle { get; set; }
        string TileToolTip { get; set; }
    }

}