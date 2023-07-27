using Prism.Mvvm;
using System;
using System.Timers;

namespace VisualHFT.Model
{
    public enum eSESSIONSTATUS
    {
        PRICE_CONNECTED_ORDER_DISCONNECTED,
        PRICE_DSICONNECTED_ORDER_CONNECTED,
        BOTH_CONNECTED,
        BOTH_DISCONNECTED
    };


    public class ProviderVM : BindableBase
    {
        private readonly Timer _timer;
        private string _providerName;
        private int _providerID;
        private eSESSIONSTATUS _status;
        private DateTime? _lastUpdated;
        private readonly int _MILLISECONDS_HEART_BEAT = 5000;
        public ProviderVM()
        {
            _timer = new Timer(interval: 5000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RaisePropertyChanged(nameof(StatusImage));
            RaisePropertyChanged(nameof(Tooltip));
        }

        public ProviderVM(ProviderVM provider)
        {
            _providerID = provider.ProviderID;
            _providerName = provider.ProviderName;
            _status = provider.Status;
        }
        public string ProviderName
        {
            get => _providerName;
            set => SetProperty(ref _providerName, value);
        }
        public int ProviderID
        {
            get => _providerID;
            set => SetProperty(ref _providerID, value); 
        }
        public eSESSIONSTATUS Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                RaisePropertyChanged(nameof(StatusImage));
                RaisePropertyChanged(nameof(Tooltip));
            }
        }
        public string StatusImage
        {
            get
            {
                if (_lastUpdated.HasValue && DateTime.Now.Subtract(_lastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "/Images/imgRedBall.png";
                }
                else if (_status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "/Images/imgGreenBall.png";
                else if (_status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "/Images/imgRedBall.png";
                else
                    return "/Images/imgYellowBall.png";
            }
        }
        public string Tooltip
        {
            get
            {
                if (_lastUpdated.HasValue && DateTime.Now.Subtract(_lastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "Not receving heart-beat";
                }
                else if (_status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "Connected";
                else if (_status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "Disconnected";
                else if (_status == eSESSIONSTATUS.PRICE_CONNECTED_ORDER_DISCONNECTED)
                    return "Price connected. Order disconnected";
                else if (_status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                    return "Price disconnected. Order connected";
                else
                    return "";
            }
        }
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

    }
}
