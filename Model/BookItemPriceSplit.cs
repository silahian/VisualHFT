using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace VisualHFT.Model
{
    public class BookItemPriceSplit : INotifyPropertyChanged, ICloneable
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

        private string _lastDecimal = "";
        private string _nextTwoDecimals = "";
        private string _rest = "";
        private string _size = "";
        private double _price = 0;

        public void SetNumber(double price, double size, int symbolDecimalPlaces)
        {
            _price = price;
            if (price != 0)
            {
                try
                {
                    string sPrice = string.Format("{0:N" + symbolDecimalPlaces + "}", price);
                    if (symbolDecimalPlaces > 0)
                    {
                        _lastDecimal = sPrice.Last().ToString();
                        _nextTwoDecimals = sPrice.Substring(sPrice.Length - 3, 2);
                        _rest = sPrice.Substring(0, sPrice.Length - 3);
                    }
                    else
                    {
                        _rest = sPrice.Split(',')[0];
                        _nextTwoDecimals = sPrice.Split(',')[1];
                    }
                    _size = Helpers.HelperCommon.GetKiloFormatter(size);
                }
                catch
                {
                    _lastDecimal = "-";
                    _nextTwoDecimals = "-";
                    _rest = "-";
                    _size = "-";
                }
            }


            if (price == 0)
            {
                _lastDecimal = "";
                _nextTwoDecimals = "";
                _rest = "";
                _size = "";
            }
        }
        public void RaiseUIThread()
        {
            RaisePropertyChanged("LastDecimal");
            RaisePropertyChanged("NextTwoDecimals");
            RaisePropertyChanged("Rest");
            RaisePropertyChanged("Size");
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string LastDecimal
        {
            get
            {
                return _lastDecimal;
            }

            set
            {
                if (_lastDecimal != value)
                {
                    _lastDecimal = value;
                    RaisePropertyChanged("LastDecimal");
                }
            }
        }

        public string NextTwoDecimals
        {
            get
            {
                return _nextTwoDecimals;
            }

            set
            {
                if (_nextTwoDecimals != value)
                {
                    _nextTwoDecimals = value;
                    RaisePropertyChanged("NextTwoDecimals");
                }
            }
        }
        public string Rest
        {
            get
            {
                return _rest;
            }

            set
            {
                if (_rest != value)
                {
                    _rest = value;
                    RaisePropertyChanged("Rest");
                }
            }
        }
        public string Size
        {
            get
            {
                return _size;
            }

            set
            {
                if (_size != value)
                {
                    _size = value;
                    RaisePropertyChanged("Size");
                }
            }
        }
        public double Price { get { return _price; } }
    }
}
