using System;

namespace VisualHFT.Model
{
    public class OrderBookLevel
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => _date = value;
        }

        private double _dateIndex;
        public double DateIndex
        {
            get => _dateIndex;
            set => _dateIndex = value;
        }

        private double _price;
        public double Price
        {
            get => _price;
            set => _price = value;
        }

        private double _size;
        public double Size
        {
            get => _size;
            set => _size = value;
        }
    }
}
