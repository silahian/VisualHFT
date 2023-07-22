using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace VisualHFT.Model
{
    public class PlotInfoPriceChart : BindableBase
    {
        public DateTime Date { get; set; }
        public double Volume { get; set; }

        double _midPrice;
        public double MidPrice
        {
            get => _midPrice;
            set => SetProperty(ref _midPrice, value);
        }
        double _bidPrice;
        public double BidPrice
        {
            get => _bidPrice;
            set => SetProperty(ref _bidPrice, value);
        }

        double _askPrice;
        public double AskPrice
        {
            get => _askPrice;
            set => SetProperty(ref _askPrice, value);
        }

        double? _buyActiveOrder;
        double? _sellActiveOrder;
        public double? BuyActiveOrder
        {
            get => _buyActiveOrder;
            set => SetProperty(ref _buyActiveOrder, value);
        }
        public double? SellActiveOrder
        {
            get => _sellActiveOrder;
            set => SetProperty(ref _sellActiveOrder, value);
        }

        public List<OrderBookLevel> BidOrders { get; set; }
        public List<OrderBookLevel> AskOrders { get; set; }
        public Brush StrokeAsk { get; set; }
        public Brush StrokeMiddle { get; set; }
        public Brush StrokeBid { get; set; }

    }
}
