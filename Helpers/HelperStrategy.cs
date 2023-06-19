using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperStrategy: ObservableCollection<string>
    {
        protected object _LOCK = new object();
        public HelperStrategy()
        {}

        public void UpdateData(List<StrategyVM> data)
        {
            lock (_LOCK)
            {
                if (Application.Current == null)
                    return;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    foreach (StrategyVM vm in data)
                    {
                        if (!this.Any(x => x == vm.StrategyCode))
                        {
                            this.Add(vm.StrategyCode);
                        }
                    }
                }));
            }
        }
    }
}
