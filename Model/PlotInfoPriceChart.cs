using System;
using System.Collections.Generic;
using VisualHFT.Commons.Model;

namespace VisualHFT.Model
{
    public partial class PlotInfoPriceChart : IResettable
    {
        protected object _objectAskLevels = new object();
        protected object _objectBidLevels = new object();


        protected List<OrderBookLevel> _askLevelOrders;
        protected List<OrderBookLevel> _bidsLevelOrders;

        public PlotInfoPriceChart()
        {
            _askLevelOrders = new List<OrderBookLevel>();
            _bidsLevelOrders = new List<OrderBookLevel>();
        }
        public DateTime Date { get; set; }
        public double Volume { get; set; }
        public double Spread { get; set; }
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

        public List<OrderBookLevel> AskLevelOrders
        {
            get
            {
                lock (_objectAskLevels)
                {
                    return _askLevelOrders;
                }
            }
            set
            {
                lock (_objectAskLevels)
                {
                    _askLevelOrders = value;
                }
            }
        }
        public List<OrderBookLevel> BidLevelOrders
        {
            get
            {
                lock (_objectBidLevels)
                {
                    return _bidsLevelOrders;
                }
            }
            set
            {
                lock (_objectBidLevels)
                {
                    _bidsLevelOrders = value;
                }
            }
        }

        public void Reset()
        {
            Date = DateTime.MinValue;
            Volume = 0;
            _midPrice = 0;
            _bidPrice = 0;
            _askPrice = 0;
            _buyActiveOrder = 0;
            _sellActiveOrder = 0;

            lock (_objectBidLevels)
                _bidsLevelOrders?.Clear();
            lock (_objectAskLevels)
                _askLevelOrders?.Clear();
        }

    }
}
