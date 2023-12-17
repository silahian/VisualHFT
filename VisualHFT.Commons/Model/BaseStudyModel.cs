using System;

namespace VisualHFT.Model
{
    public partial class BaseStudyModel 
    {
        private DateTime _timestamp;
        private decimal _value;
        private string _valueFormatted;
        private string _valueColor = null;
        private decimal _marketMidPrice;

        public BaseStudyModel()
        {
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }
        public decimal Value
        {
            get => _value;
            set => _value = value;
        }
        public string ValueFormatted
        {
            get => _valueFormatted;
            set => _valueFormatted = value;
        }
        public string ValueColor 
        { 
            get => _valueColor; 
            set => _valueColor = value;
        }
        public decimal MarketMidPrice
        {
            get => _marketMidPrice;
            set => _marketMidPrice = value;
        }
        public string Tooltip { get; set; }
    }
}
