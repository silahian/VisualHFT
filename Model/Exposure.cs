using System.ComponentModel;

namespace VisualHFT.Model
{

    public class Exposure : INotifyPropertyChanged
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
        string _symbol;
        string _strategyName;
        double _sizeExposed;
        double _UnrealizedPL;
        string _exposureRowColor;
        public Exposure()
        {

        }
        public Exposure(Exposure p)
        {
            _symbol = p._symbol;
            _strategyName = p._strategyName;
            _sizeExposed = p._sizeExposed;
            _UnrealizedPL = p._UnrealizedPL;
        }
        public string Symbol
        {
            get
            {
                return _symbol;
            }

            set
            {
                _symbol = value;
                RaisePropertyChanged("Symbol");
            }
        }
        public string StrategyName
        {
            get
            {
                return _strategyName;
            }

            set
            {
                _strategyName = value;
                RaisePropertyChanged("StrategyName");
            }
        }
        public double SizeExposed
        {
            get
            {
                return _sizeExposed;
            }

            set
            {
                _sizeExposed = value;

                RaisePropertyChanged("SizeExposed");
            }
        }
        public double UnrealizedPL
        {
            get
            {
                return _UnrealizedPL;
            }
            set
            {
                _UnrealizedPL = value;
                if (value == 0)
                    _exposureRowColor = "White";
                else if (value < 0 )
                    _exposureRowColor = "Red";
                else
                    _exposureRowColor = "Green";
                RaisePropertyChanged("UnrealizedPL");
                RaisePropertyChanged("ExposureRowColor");
            }

        }
        public string ExposureRowColor
        {
            get { return _exposureRowColor; }
        }

    }
}
