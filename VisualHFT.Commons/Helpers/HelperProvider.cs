using VisualHFT.Model;
using System.Collections.Concurrent;
using VisualHFT.Enums;
using VisualHFT.Commons.Helpers;

namespace VisualHFT.Helpers
{
    public class HelperProvider : ConcurrentDictionary<int, Model.Provider>, IDisposable
    {
        // We use a timer to check when was the last time we received an update.
        // If we have not received any update in that timespan, we trigger an OnHeartBeatFail
        // The timespan we've chosen to check this is 30,000 milliseconds (30 sec)
        private int _MILLISECONDS_HEART_BEAT = 30000;
        private readonly System.Timers.Timer _timer_check_heartbeat;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly HelperProvider instance = new HelperProvider();
        public static HelperProvider Instance => instance;


        public event EventHandler<Provider> OnDataReceived;

        public event EventHandler<Provider> OnStatusChanged;

        public HelperProvider()
        {
            _timer_check_heartbeat = new System.Timers.Timer(_MILLISECONDS_HEART_BEAT);
            _timer_check_heartbeat.Elapsed += _timer_check_heartbeat_Elapsed;
            _timer_check_heartbeat.Start();
        }
        private void _timer_check_heartbeat_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var x in this)
            {
                if (x.Value.Status == eSESSIONSTATUS.DISCONNECTED || x.Value.Status == eSESSIONSTATUS.DISCONNECTED_FAILED)
                    continue;


                if (HelperTimeProvider.Now.Subtract(x.Value.LastUpdated).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    var _msg = $"{x.Value.ProviderName} hasn't received any provider's heartbeat. Last message received: {x.Value.LastUpdated}";
                    HelperNotificationManager.Instance.AddNotification(x.Value.ProviderName, _msg, HelprNorificationManagerTypes.WARNING, HelprNorificationManagerCategories.PLUGINS, null);
                    log.Warn(_msg);

                    x.Value.Status = eSESSIONSTATUS.CONNECTED_WITH_WARNINGS;
                    OnStatusChanged?.Invoke(this, x.Value);
                }
            }
        }
        public List<Model.Provider> ToList()
        {
            return this.Values.ToList();
        }

        protected virtual void RaiseOnDataReceived(Provider provider)
        {
            EventHandler<Provider> _handler = OnDataReceived;
            if (_handler != null)
            {
                _handler(this, provider);
            }
        }
        public void UpdateData(IEnumerable<VisualHFT.Model.Provider> providers)
        {
            foreach (var provider in providers)
            {
                if (UpdateDataInternal(provider))
                    RaiseOnDataReceived(provider);//Raise all provs allways
            }
        }

        public void UpdateData(VisualHFT.Model.Provider provider)
        {
            if (UpdateDataInternal(provider))
                RaiseOnDataReceived(provider);//Raise all provs allways
        }
        private bool UpdateDataInternal(VisualHFT.Model.Provider provider)
        {
            if (provider != null)
            {
                //Check provider
                if (!this.ContainsKey(provider.ProviderCode))
                {
                    provider.LastUpdated = HelperTimeProvider.Now;
                    return this.TryAdd(provider.ProviderCode, provider);
                }
                else
                {
                    bool hasStatusChanged = provider.Status != this[provider.ProviderCode].Status;
                    this[provider.ProviderCode].LastUpdated = HelperTimeProvider.Now;
                    this[provider.ProviderCode].Status = provider.Status;
                    this[provider.ProviderCode].Plugin = provider.Plugin;
                    if (hasStatusChanged) //do something with the status that has changed
                    {
                        OnStatusChanged?.Invoke(this, this[provider.ProviderCode]);
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
