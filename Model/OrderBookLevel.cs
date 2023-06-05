using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VisualHFT.Model
{
    public class OrderBookLevel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private DateTime _date;
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date != value)
                {
                    _date = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _dateIndex;
        public double DateIndex
        {
            get { return _dateIndex; }
            set {
                if (_dateIndex != value)
                {
                    _dateIndex = value;
                    RaisePropertyChanged();
                }
            }
        }

        private double _price;
        public double Price
        {
            get { return _price ; }
            set {
                if (_price != value)
                {
                    _price = value;
                    RaisePropertyChanged();
                }

            }
        }

        private double _size;
        public double Size
        {
            get { return _size; }
            set {
                if (_size != value)
                {
                    _size = value;
                    RaisePropertyChanged();
                }

            }
        }
    }
}
