using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace VisualHFT.Model
{
    public class PlotInfoPriceChart : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }        
        public DateTime Date { get; set; }
        public double Volume { get; set; }

        double _midPrice;
        public double MidPrice
        {
            get { return _midPrice; }
            set
            {
                if (_midPrice != value)
                {
                    _midPrice = value;
                    RaisePropertyChanged();
                }
            }
        }
        double _bidPrice;
        public double BidPrice
        {
            get { return _bidPrice; }
            set
            {
                if (_bidPrice != value)
                {
                    _bidPrice = value;
                    RaisePropertyChanged();
                }
            }
        }

        double _askPrice;
        public double AskPrice
        {
            get { return _askPrice; }
            set
            {
                if (_askPrice != value)
                {
                    _askPrice = value;
                    RaisePropertyChanged();
                }
            }
        }

		double? _buyActiveOrder;
		double? _sellActiveOrder;
		public double? BuyActiveOrder
		{
			get { return _buyActiveOrder; }
			set
			{
				if (_buyActiveOrder != value)
				{
					_buyActiveOrder = value;
					RaisePropertyChanged();
				}
			}
		}
		public double? SellActiveOrder
		{
			get { return _sellActiveOrder; }
			set
			{
				if (_sellActiveOrder != value)
				{
					_sellActiveOrder = value;
					RaisePropertyChanged();
				}
			}
		}




		public Brush StrokeAsk { get; set; }
		public Brush StrokeMiddle { get; set; }
		public Brush StrokeBid { get; set; }

	}
}
