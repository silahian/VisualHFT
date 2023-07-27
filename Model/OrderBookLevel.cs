using Prism.Mvvm;
using System;

namespace VisualHFT.Model
{
    public class OrderBookLevel : BindableBase
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private double _dateIndex;
        public double DateIndex
        {
            get => _dateIndex;
            set => SetProperty(ref _dateIndex, value);
        }

        private double _price;
        public double Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        private double _size;
        public double Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }
    }
}
