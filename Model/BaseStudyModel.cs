using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace VisualHFT.Model
{
    public class BaseStudyModel : BindableBase
    {
        private DateTime _timestamp;
        private decimal _value;
        private string _valueFormatted;
        private string _valueColor = null;
        private decimal _marketMidPrice;

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }
        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        public string ValueFormatted
        {
            get => _valueFormatted;
            set => SetProperty(ref _valueFormatted, value);
        }
        public string ValueColor { get => _valueColor; set => SetProperty(ref _valueColor, value); }
        public decimal MarketMidPrice
        {
            get => _marketMidPrice;
            set => SetProperty(ref _marketMidPrice, value);
        }
    }
}
