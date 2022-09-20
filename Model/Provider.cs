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


    public class Provider : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        string providerName;
        int providerID;
        eSESSIONSTATUS status;
        public Provider()
        {

        }
        public Provider(Provider p)
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
                if (status == eSESSIONSTATUS.BOTH_CONNECTED)
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
                if (status == eSESSIONSTATUS.BOTH_CONNECTED)
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


    }
}
