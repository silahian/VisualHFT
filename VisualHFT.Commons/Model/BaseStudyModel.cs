using System;

namespace VisualHFT.Model
{
    public partial class BaseStudyModel
    {
        private DateTime _timestamp;
        private decimal _value;
        private string _valueFormatted;
        private string _format;
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

        public string Format
        {
            get => _format;
            set => _format = value;
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
        public string Tag { get; set; }

        public void copyFrom(BaseStudyModel e)
        {
            _timestamp = e.Timestamp;
            _value = e.Value;
            _format = e.Format;
            _valueFormatted = e.ValueFormatted;
            _valueColor = e.ValueColor;
            _marketMidPrice = e.MarketMidPrice;
            Tooltip = e.Tooltip;
        }

        public void Reset()
        {
            _timestamp = DateTime.MinValue;
            _value = 0;
            _format = "";
            _valueFormatted = "";
            _valueColor = "";
            _marketMidPrice = 0;
            Tooltip = "";
        }
    }
}