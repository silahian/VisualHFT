using System;
using System.ComponentModel;

namespace VisualHFT.ViewModel.Model
{
    public class Provider: VisualHFT.Model.Provider, INotifyPropertyChanged
    {
        private int _MILLISECONDS_HEART_BEAT = 5000;

        public new int ProviderID
        {
            get { return base.ProviderID; }
            set
            {
                base.ProviderID = value;
                OnPropertyChanged(nameof(ProviderID));
            }
        }

        public new int ProviderCode
        {
            get { return base.ProviderCode; }
            set
            {
                base.ProviderCode = value;
                OnPropertyChanged(nameof(ProviderCode));
            }
        }

        public new string ProviderName
        {
            get { return base.ProviderName; }
            set
            {
                base.ProviderName = value;
                OnPropertyChanged(nameof(ProviderName));
            }
        }

        public new eSESSIONSTATUS Status
        {
            get { return base.Status; }
            set
            {
                base.Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public DateTime? LastUpdated { get; set; }

        public string StatusImage
        {
            get
            {
                if (LastUpdated.HasValue && DateTime.Now.Subtract(LastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "/Images/imgRedBall.png";
                }
                else if (Status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "/Images/imgGreenBall.png";
                else if (Status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "/Images/imgRedBall.png";
                else
                    return "/Images/imgYellowBall.png";
            }
        }
        public string Tooltip
        {
            get
            {
                if (LastUpdated.HasValue && DateTime.Now.Subtract(LastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "Not receving heart-beat";
                }
                else if (Status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "Connected";
                else if (Status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "Disconnected";
                else if (Status == eSESSIONSTATUS.PRICE_CONNECTED_ORDER_DISCONNECTED)
                    return "Price connected. Order disconnected";
                else if (Status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                    return "Price disconnected. Order connected";
                else
                    return "";
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Provider()
        {
                
        }
        public Provider(VisualHFT.Model.Provider p)
        {
            this.ProviderID = p.ProviderID;
            this.ProviderCode = p.ProviderCode;
            this.ProviderName = p.ProviderName;
            this.LastUpdated =   DateTime.Now;
        }
        public void CheckValuesUponHeartbeatReceived()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusImage));
            OnPropertyChanged(nameof(Tooltip));
            
        }
    }
}
