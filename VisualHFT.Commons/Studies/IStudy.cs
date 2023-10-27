using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;
using VisualHFT.UserSettings;

namespace VisualHFT.Commons.Studies
{
    public interface IStudy: IDisposable
    {
        public event EventHandler<decimal> OnAlertTriggered;
        public event EventHandler<BaseStudyModel> OnCalculated;

        Task StartAsync();
        Task StopAsync();
        string TileTitle { get; set; }
        string TileToolTip { get; set; }
    }

}
