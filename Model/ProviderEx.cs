using System;
using System.ComponentModel;

namespace VisualHFT.Model
{
    public class ProviderEx : Provider, INotifyPropertyChanged
    {
        private DateTime? _lastUpdated;
        private eSESSIONSTATUS _sessionStatus;
        private int _MILLISECONDS_HEART_BEAT = 5000;
        private int _providerID;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set
            {
                if (_lastUpdated != value)
                {
                    _lastUpdated = value;
                    OnPropertyChanged(nameof(LastUpdated));
                    OnPropertyChanged(nameof(StatusImage));
                    OnPropertyChanged(nameof(Tooltip));
                }
            }

        }
        public eSESSIONSTATUS Status
        { 
            get { return _sessionStatus; }
            set
            {
                if (_sessionStatus != value)
                {
                    _sessionStatus = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(StatusImage));
                    OnPropertyChanged(nameof(Tooltip));
                }
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
                else if (_sessionStatus == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "/Images/imgGreenBall.png";
                else if (_sessionStatus == eSESSIONSTATUS.BOTH_DISCONNECTED)
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
                else if (_sessionStatus == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "Connected";
                else if (_sessionStatus == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "Disconnected";
                else if (_sessionStatus == eSESSIONSTATUS.PRICE_CONNECTED_ORDER_DISCONNECTED)
                    return "Price connected. Order disconnected";
                else if (_sessionStatus == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                    return "Price disconnected. Order connected";
                else
                    return "";
            }
        }
        public int ProviderID
        {
            set { _providerID = value; this.ProviderCode = value; }
        }
        public void CheckValuesUponHeartbeatReceived()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusImage));
            OnPropertyChanged(nameof(Tooltip));
        }
    }
}
