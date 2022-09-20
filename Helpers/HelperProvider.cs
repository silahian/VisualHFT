using VisualHFT.Model;
using VisualHFT.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperProvider: ConcurrentDictionary<int, Provider>
    {
        public event EventHandler<Provider> OnDataReceived;

        public HelperProvider()
        {}
        ~HelperProvider()
        { }


        protected virtual void RaiseOnDataReceived(List<Provider> providers)
        {
            EventHandler<Provider> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    foreach (var p in providers)
                        _handler(this, p);
                }));
            }
        }


        public void UpdateData(IEnumerable<Provider> providers)
        {
            List<Provider> toUpdate = new List<Provider>();
            foreach (var provider in providers)
            {
                if (UpdateData(provider))
                    toUpdate.Add(provider);
            }
            if (toUpdate.Any())
                RaiseOnDataReceived(toUpdate);
        }
        private bool UpdateData(Provider provider)
        {

            if (provider != null)
            {
                //Check provider
                if (!this.ContainsKey(provider.ProviderID))
                {
                    return this.TryAdd(provider.ProviderID, provider);
                }
			}
            return false;
        }

    }
}
