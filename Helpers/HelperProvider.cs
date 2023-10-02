using VisualHFT.ViewModel.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace VisualHFT.Helpers
{
    public class HelperProvider: ConcurrentDictionary<int, Provider>, IDisposable
    {
        private const int HEARTBEAT_INTERVAL = 5000; // Interval for polling the database
        private readonly System.Timers.Timer _timer_check_heartbeat;

        public event EventHandler<Provider> OnDataReceived;
        public event EventHandler<Provider> OnHeartBeatFail;
        public HelperProvider()
        {
            _timer_check_heartbeat = new System.Timers.Timer(HEARTBEAT_INTERVAL);
            _timer_check_heartbeat.Elapsed += _timer_check_heartbeat_Elapsed;
            _timer_check_heartbeat.Start();
        }

        private void _timer_check_heartbeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach(var x in this)
            {
                x.Value.CheckValuesUponHeartbeatReceived();
                RaiseOnHeartBeatFail(x.Value);
            }
        }

        protected virtual void RaiseOnHeartBeatFail(Provider provider)
        {
            EventHandler<Provider> handler = OnHeartBeatFail;
            if (handler != null && Application.Current != null)
            {
                handler(this, provider);
            }
        }
        protected virtual void RaiseOnDataReceived(List<Provider> providers)
        {
            EventHandler<Provider> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                foreach (var p in providers)
                    _handler(this, p);
            }
        }
        public void UpdateData(IEnumerable<VisualHFT.Model.Provider> providers)
        {
            var provUpdated = new List<Provider>();
            foreach (var provider in providers)
            {
                if (UpdateData(provider))
                    provUpdated.Add(this[provider.ProviderCode]);
            }
            if (provUpdated.Any())
            {
                RaiseOnDataReceived(provUpdated);//Raise all provs allways
            }
            
        }
        private bool UpdateData(VisualHFT.Model.Provider provider)
        {

            if (provider != null)
            {
                //Check provider
                if (!this.ContainsKey(provider.ProviderCode))
                {
                    return this.TryAdd(provider.ProviderCode, new Provider(provider));
                }
                else
                {
                    this[provider.ProviderCode].LastUpdated = DateTime.Now;
                    this[provider.ProviderCode].Status = provider.Status;
                    this[provider.ProviderCode].Plugin = provider.Plugin;
                }
			}
            return false;
        }
        public ObservableCollection<VisualHFT.ViewModel.Model.Provider> CreateObservableCollection()
        {
            return new ObservableCollection<VisualHFT.ViewModel.Model.Provider>(this.Values.ToList());
        }
        public void Dispose()
        {
            _timer_check_heartbeat?.Stop();
            _timer_check_heartbeat?.Dispose();
        }
    }
}
