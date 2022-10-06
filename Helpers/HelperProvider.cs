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
    public class HelperProvider: ConcurrentDictionary<int, ProviderVM>
    {
        public event EventHandler<ProviderVM> OnDataReceived;

        public HelperProvider()
        {}
        ~HelperProvider()
        { }


        protected virtual void RaiseOnDataReceived(List<ProviderVM> providers)
        {
            EventHandler<ProviderVM> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                    foreach (var p in providers)
                        _handler(this, p);
                }));
            }
        }


        public void UpdateData(IEnumerable<ProviderVM> providers)
        {
            List<ProviderVM> toUpdate = new List<ProviderVM>();
            foreach (var provider in providers)
            {
                if (UpdateData(provider))
                    toUpdate.Add(provider);
            }
            if (toUpdate.Any())
                RaiseOnDataReceived(toUpdate);
        }
        private bool UpdateData(ProviderVM provider)
        {

            if (provider != null)
            {
                provider.LastUpdated = DateTime.Now; 
                //Check provider
                if (!this.ContainsKey(provider.ProviderID))
                {
                    UpdateDB(provider);
                    return this.TryAdd(provider.ProviderID, provider);
                }
                else
                {
                    if (this[provider.ProviderID].Status != provider.Status || this[provider.ProviderID].LastUpdated != provider.LastUpdated)
                    {
                        this[provider.ProviderID].Status = provider.Status;
                        this[provider.ProviderID].LastUpdated = provider.LastUpdated;
                        return true;
                    }
                }
			}
            return false;
        }
        private void UpdateDB(ProviderVM provider)
        {
            using (var db = new HFTEntities())
            {
                var exists = db.Providers.Where(x => x.ProviderCode == provider.ProviderID).FirstOrDefault();
                if (exists == null)
                {
                    db.Providers.Add(new Provider()
                    {
                        ProviderCode = provider.ProviderID,
                        ProviderName = provider.ProviderName
                    });                    
                    var ret = db.SaveChanges();
                }
            }
        }
    }
}
