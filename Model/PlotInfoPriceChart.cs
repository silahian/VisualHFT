using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using VisualHFT.Commons.Model;
using VisualHFT.Helpers;

namespace VisualHFT.Model
{
    public partial class PlotInfoPriceChart : IResettable
    {
        public PlotInfoPriceChart()
        {
            AskLevelOrders = new List<OrderBookLevel>();
            BidLevelOrders = new List<OrderBookLevel>();
        }
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
        public List<OrderBookLevel> AskLevelOrders { get; set; }
        public List<OrderBookLevel> BidLevelOrders { get; set; }

        public void Reset()
        {
            Date = DateTime.MinValue;
            Volume = 0;
            _midPrice = 0;
            _bidPrice = 0;
            _askPrice = 0;
            _buyActiveOrder = 0;
            _sellActiveOrder = 0;

            AskLevelOrders?.Clear();
            BidLevelOrders?.Clear();
        }
    }
}
