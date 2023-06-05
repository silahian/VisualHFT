using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VisualHFT.Model
{
    public class PlotInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        double _value;
        public DateTime Date { get; set; }
        public double Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
