using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace VisualHFT.Model
{
    public class PlotInfoPriceChart
    {
        public DateTime Date { get; set; }
        public double Volume { get; set; }

        double _midPrice;
        public double MidPrice
        {
            get => _midPrice;
            set => _midPrice = value;
        }
        double _bidPrice;
        public double BidPrice
        {
            get => _bidPrice;
            set => _bidPrice = value;
        }

        double _askPrice;
        public double AskPrice
        {
            get => _askPrice;
            set => _askPrice = value;
        }

        double? _buyActiveOrder;
        double? _sellActiveOrder;
        public double? BuyActiveOrder
        {
            get => _buyActiveOrder;
            set => _buyActiveOrder = value;
        }
        public double? SellActiveOrder
        {
            get => _sellActiveOrder;
            set => _sellActiveOrder = value;
        }

        public List<OrderBookLevel> BidOrders { get; set; }
        public List<OrderBookLevel> AskOrders { get; set; }
        public Brush StrokeAsk { get; set; }
        public Brush StrokeMiddle { get; set; }
        public Brush StrokeBid { get; set; }

    }
}
