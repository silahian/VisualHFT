using VisualHFT.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace VisualHFT.Helpers
{
    public class HelperProvider: ConcurrentDictionary<int, ProviderEx>, IDisposable
    {
        private const int HEARTBEAT_INTERVAL = 5000; // Interval for polling the database
        private readonly System.Timers.Timer _timer_check_heartbeat;

        public event EventHandler<ProviderEx> OnDataReceived;
        public event EventHandler<ProviderEx> OnHeartBeatFail;
        public HelperProvider()
        {
            _timer_check_heartbeat = new System.Timers.Timer(HEARTBEAT_INTERVAL);
            _timer_check_heartbeat.Elapsed += _timer_check_heartbeat_Elapsed; ;
            _timer_check_heartbeat.Start();
        }

        private void _timer_check_heartbeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach(var x in this)
            {
                x.Value.CheckValuesUponHeartbeatReceived();
                if (x.Value.Status == eSESSIONSTATUS.BOTH_DISCONNECTED || x.Value.Status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                {
                    RaiseOnHeartBeatFail(x.Value);
                }
            }
        }

        protected virtual void RaiseOnHeartBeatFail(ProviderEx provider)
        {
            EventHandler<ProviderEx> handler = OnHeartBeatFail;
            if (handler != null && Application.Current != null)
            {
                handler(this, provider);
            }
        }
        protected virtual void RaiseOnDataReceived(List<ProviderEx> providers)
        {
            EventHandler<ProviderEx> _handler = OnDataReceived;
            if (_handler != null && Application.Current != null)
            {
                /*Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => {
                }));*/
                foreach (var p in providers)
                    _handler(this, p);
            }
        }
        public void UpdateData(IEnumerable<ProviderEx> providers)
        {
            foreach (var provider in providers)
            {
                UpdateData(provider);
            }
            RaiseOnDataReceived(this.Select(x=>x.Value).ToList());//Raise all provs allways
        }
        private bool UpdateData(ProviderEx provider)
        {

            if (provider != null)
            {
                provider.LastUpdated = DateTime.Now; 
                //Check provider
                if (!this.ContainsKey(provider.ProviderCode))
                {
                    return this.TryAdd(provider.ProviderCode, provider);
                }
                else
                {
                    if (this[provider.ProviderCode].Status != provider.Status || this[provider.ProviderCode].LastUpdated != provider.LastUpdated)
                    {
                        this[provider.ProviderCode].Status = provider.Status;
                        this[provider.ProviderCode].LastUpdated = provider.LastUpdated;
                        return true;
                    }
                }
			}
            return false;
        }

        public void Dispose()
        {
            _timer_check_heartbeat?.Stop();
            _timer_check_heartbeat?.Dispose();
        }
    }
}
