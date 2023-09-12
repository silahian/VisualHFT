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

        public BaseStudyModel(bool isBindable = true)
        {
            IsBindable = isBindable;
        }

        public bool IsBindable { get; set; } = true;
        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                if (IsBindable)
                    SetProperty(ref _timestamp, value);
                else
                    _timestamp = value;
            }
        }
        public decimal Value
        {
            get => _value;
            set {
                if (IsBindable)
                    SetProperty(ref _value, value);
                else
                    _value = value;
            }
        }
        public string ValueFormatted
        {
            get => _valueFormatted;
            set 
            {
                if (IsBindable)
                    SetProperty(ref _valueFormatted, value);
                else
                    _valueFormatted = value;
            }
        }
        public string ValueColor 
        { 
            get => _valueColor; 
            set
            {
                if (IsBindable)
                    SetProperty(ref _valueColor, value);
                else
                    _valueColor = value;
            }
        }
        public decimal MarketMidPrice
        {
            get => _marketMidPrice;
            set
            {
                if (IsBindable)
                    SetProperty(ref _marketMidPrice, value);
                else
                    _marketMidPrice = value;
                    
            }
        }
    }
}
