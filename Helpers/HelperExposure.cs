using VisualHFT.Model;
using VisualHFT.ViewModel;
using log4net.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperExposure: ConcurrentDictionary<string, Exposure>
    {
        public event EventHandler<Exposure> OnDataReceived;

        public HelperExposure()
        {}
        ~HelperExposure(){}

        protected virtual void RaiseOnDataReceived(List<Exposure> exposures)
        {
            EventHandler<Exposure> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    foreach (var exposure in exposures)
                        _handler(this, exposure);
                }));
            }
        }

        public void UpdateData(IEnumerable<Exposure> exposures)
        {
            List<Exposure> toUpdate = new List<Exposure>();
            foreach (var e in exposures)
            {
                if (UpdateData(e))
                    toUpdate.Add(e);
            }
            if (toUpdate.Any())
                RaiseOnDataReceived(toUpdate);
        }
        private bool UpdateData(Exposure exposure)
        {
            if (exposure != null)
            {
                string _key = exposure.StrategyName + exposure.Symbol;
                //Check provider
                if (!this.ContainsKey(_key))
                {
                    return this.TryAdd(_key, exposure);
                }
                else
                {
                    bool hasChanged = false;
                    //UPPDATE
                    if (this[_key].SizeExposed != exposure.SizeExposed)
                    {
                        this[_key].SizeExposed = exposure.SizeExposed;
                        hasChanged = true;
                    }
                    if (this[_key].UnrealizedPL != exposure.UnrealizedPL)
                    {
                        this[_key].UnrealizedPL = exposure.UnrealizedPL;
                        hasChanged = true;
                    }
                    return hasChanged;
                }
            }
            return false;
        }

    }
}
