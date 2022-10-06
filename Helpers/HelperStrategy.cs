using VisualHFT.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
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
