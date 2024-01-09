using VisualHFT.Model;
using System.Collections.Concurrent;

namespace VisualHFT.Helpers
{
    public class HelperProvider: ConcurrentDictionary<int, Model.Provider>, IDisposable
    {
        private int _MILLISECONDS_HEART_BEAT = 5000;
        private readonly System.Timers.Timer _timer_check_heartbeat;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly HelperProvider instance = new HelperProvider();
        public static HelperProvider Instance => instance;


        public event EventHandler<Provider> OnDataReceived;
        public event EventHandler<Provider> OnHeartBeatFail;

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
                if (HelperTimeProvider.Now.Subtract(x.Value.LastUpdated).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    x.Value.Status = eSESSIONSTATUS.BOTH_DISCONNECTED;
                    OnHeartBeatFail?.Invoke(this, x.Value);
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
        public void HeartbeatFailed(Provider provider)
        {
            this[provider.ProviderID].Status = provider.Status;
            OnHeartBeatFail?.Invoke(this, provider);
        }
        public void UpdateData(IEnumerable<VisualHFT.Model.Provider> providers)
        {
            foreach (var provider in providers)
            {
                if (UpdateData(provider))
                    RaiseOnDataReceived(provider);//Raise all provs allways
            }
            
        }
        private bool UpdateData(VisualHFT.Model.Provider provider)
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
                    this[provider.ProviderCode].LastUpdated = HelperTimeProvider.Now;
                    this[provider.ProviderCode].Status = provider.Status;
                    this[provider.ProviderCode].Plugin = provider.Plugin;
                    if (provider.Status == eSESSIONSTATUS.BOTH_DISCONNECTED || provider.Status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                        OnHeartBeatFail?.Invoke(this, provider);

                    return true;
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
