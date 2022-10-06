using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public enum eSESSIONSTATUS
    {
        PRICE_CONNECTED_ORDER_DISCONNECTED,
        PRICE_DSICONNECTED_ORDER_CONNECTED,
        BOTH_CONNECTED,
        BOTH_DISCONNECTED
    };


    public class ProviderVM : INotifyPropertyChanged
    {
        protected readonly System.Timers.Timer _timer;
        protected string providerName;
        protected int providerID;
        protected eSESSIONSTATUS status;
        protected DateTime? lastUpdated;
        protected int _MILLISECONDS_HEART_BEAT = 5000;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public ProviderVM()
        {
            _timer = new System.Timers.Timer();
            _timer.Interval = 5000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = true;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RaisePropertyChanged("StatusImage");
            RaisePropertyChanged("Tooltip");
        }

        public ProviderVM(ProviderVM p)
        {
            this.providerID = p.ProviderID;
            this.providerName = p.ProviderName;
            this.status = p.Status;
        }
        public string ProviderName
        {
            get
            {
                return providerName;
            }

            set
            {
                providerName = value;
                RaisePropertyChanged("ProviderName");
            }
        }
        public int ProviderID
        {
            get
            {
                return providerID;
            }

            set
            {
                providerID = value;
                RaisePropertyChanged("ProviderID");
            }
        }
        public eSESSIONSTATUS Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
                RaisePropertyChanged("Status");
                RaisePropertyChanged("StatusImage");
                RaisePropertyChanged("Tooltip");
            }
        }
        public string StatusImage
        {

            get
            {
                if (lastUpdated.HasValue && DateTime.Now.Subtract(lastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "/Images/imgRedBall.png";
                }
                else if (status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "/Images/imgGreenBall.png";
                else if (status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "/Images/imgRedBall.png";
                else
                    return "/Images/imgYellowBall.png";
            }
        }
        public string Tooltip
        {
            get
            {
                if (lastUpdated.HasValue && DateTime.Now.Subtract(lastUpdated.Value).TotalMilliseconds > _MILLISECONDS_HEART_BEAT)
                {
                    return "Not receving heart-beat";
                }
                else if (status == eSESSIONSTATUS.BOTH_CONNECTED)
                    return "Connected";
                else if (status == eSESSIONSTATUS.BOTH_DISCONNECTED)
                    return "Disconnected";
                else if (status == eSESSIONSTATUS.PRICE_CONNECTED_ORDER_DISCONNECTED)
                    return "Price connected. Order disconnected";
                else if (status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED)
                    return "Price disconnected. Order connected";
                else
                    return "";
            }
        }
        public DateTime? LastUpdated
        { 
            get { return lastUpdated;  }
            set
            {
                lastUpdated = value;
                RaisePropertyChanged("LastUpdated");
            }
        }

    }
}
