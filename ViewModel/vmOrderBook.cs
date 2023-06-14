using VisualHFT.Extensions;
using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;

namespace VisualHFT.ViewModel
{
    public class vmOrderBook : BindableBase
    {
        private OrderBook _orderBook;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        //private int REALTIME_ITEM_POINTS = 1000;
        private int REALTIME_ITEM_POINTS_IN_SEC = 30; //last x seconds

        private ObservableCollection<PlotInfoPriceChart> _realTimePrices;
        ObservableCollection<PlotInfoPriceChart> _realTimeSpread;
        ObservableCollection<OrderBookLevel> _realTimeOrderBookBids;
        ObservableCollection<OrderBookLevel> _realTimeOrderBookAsks;

        private ObservableCollection<ProviderVM> _providers;
        private string _selectedSymbol;
        private ProviderVM _selectedProvider=null;
        private string _layerName;
        private double _maxOrderSize = 0;

        private double _MidPoint;
        private BookItem _AskTOB = new BookItem();
        private BookItem _BidTOB = new BookItem();
        private double _Spread;
        
        private double _ChartMaximumValue_Y;
        private double _ChartMinimumValue_Y;
        private BookItemPriceSplit _BidTOB_SPLIT = null;
        private BookItemPriceSplit _AskTOB_SPLIT = null;
        private double _PercentageWidth = 1;

        private DateTime _LAST_UI_UPDATED = DateTime.Now;
        private int _PERIOD_TO_UPDATE_UI_IN_MS = 300;


        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;


            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;

        }

        private void Clear()
        {
            this.MidPoint = 0;
            this.AskTOB = new BookItem();
            this.BidTOB = new BookItem();
            this.Spread = 0;
        
            this.ChartMaximumValue_Y = 0;
            this.ChartMinimumValue_Y = 0;
            this.BidTOB_SPLIT = null;
            this.AskTOB_SPLIT = null;
            this.OrderBook = null;
            _realTimePrices = null;
            _realTimeSpread = null;
            _realTimeOrderBookBids = null;
            _realTimeOrderBookAsks = null;

            RaisePropertyChanged(nameof(RealTimeOrderBookAsks));
            RaisePropertyChanged(nameof(RealTimeOrderBookBids));
            RaisePropertyChanged(nameof(AskCummulative));
            RaisePropertyChanged(nameof(BidCummulative));
            RaisePropertyChanged(nameof(Asks));
            RaisePropertyChanged(nameof(Bids));
            RaisePropertyChanged(nameof(RealTimePrices));
            RaisePropertyChanged(nameof(RealTimeSpread));

        }
        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderID != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (_orderBook != null)
            {
                var comp = 1.0 / Math.Pow(10, e.SymbolDecimals);
                var o = _orderBook.Asks.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _orderBook.Bids.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();

                if (o!= null)
                {
                    if (o.ActiveSize != null && o.ActiveSize - e.Quantity > 0)
                        o.ActiveSize -= e.Quantity;
                    else
                        o.ActiveSize = null;
                }
            }            
        }
        private void ACTIVEORDERS_OnDataReceived(object sender, OrderVM e)
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderID != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (_orderBook != null)
            {
                var comp = 1.0 / Math.Pow(10, e.SymbolDecimals);

                var o = _orderBook.Asks.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _orderBook.Bids.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();

                if (o != null)
                {
                    if (o.ActiveSize != null)
                        o.ActiveSize += e.Quantity;
                    else
                        o.ActiveSize = e.Quantity;
                }
            }
        }
        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            if (e == null)
                return;
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderID != e.ProviderID)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;
            
            if (true)
            {
                if (_orderBook == null || _orderBook.ProviderID != e.ProviderID || _orderBook.Symbol != e.Symbol)
                {
                    _maxOrderSize = 0; //reset
                    _orderBook = e;
                    _realTimePrices = new ObservableCollection<PlotInfoPriceChart>();
                    _realTimeSpread = new ObservableCollection<PlotInfoPriceChart>();

                    _realTimeOrderBookBids = new ObservableCollection<OrderBookLevel>();
                    _realTimeOrderBookAsks = new ObservableCollection<OrderBookLevel>();

                    _AskTOB_SPLIT = new BookItemPriceSplit();
                    _BidTOB_SPLIT = new BookItemPriceSplit();
                    

                    RaisePropertyChanged(nameof(RealTimeSpread));
                    RaisePropertyChanged(nameof(RealTimePrices));
                    RaisePropertyChanged(nameof(RealTimeOrderBookBids));
                    RaisePropertyChanged(nameof(RealTimeOrderBookAsks));
                    RaisePropertyChanged(nameof(AskTOB_SPLIT));
                    RaisePropertyChanged(nameof(BidTOB_SPLIT));
                }
                if (!_orderBook.LoadData(e.Asks.ToList(), e.Bids.ToList()))
                    return; //if nothing to update, then exit

                if (DateTime.Now.Subtract(_LAST_UI_UPDATED).TotalMilliseconds > _PERIOD_TO_UPDATE_UI_IN_MS)
                {
                    RaisePropertyChanged(nameof(Bids));
                    RaisePropertyChanged(nameof(Asks));

                    RaisePropertyChanged(nameof(ChartMaximumValue_Y));                
                    RaisePropertyChanged(nameof(AskCummulative));
                    RaisePropertyChanged(nameof(BidCummulative));
                }

                #region Calculate TOB values
                var tobBid = _orderBook.GetTOB(true);
                var tobAsk = _orderBook.GetTOB(false);

                if (tobAsk != null && (tobAsk.Price != _AskTOB.Price || tobAsk.Size != _AskTOB.Size))
                {
                    this.AskTOB = tobAsk;
                    _AskTOB_SPLIT.SetNumber(tobAsk.Price, tobAsk.Size, _orderBook.DecimalPlaces);
                }
                if (tobBid != null && (tobBid.Price != _BidTOB.Price || tobBid.Size != _BidTOB.Size))
                {
                    this.BidTOB = tobBid;
                    _BidTOB_SPLIT.SetNumber(tobBid.Price, tobBid.Size, _orderBook.DecimalPlaces);
                }
                if (tobAsk != null && tobBid != null)
                {
                    if (DateTime.Now.Subtract(_LAST_UI_UPDATED).TotalMilliseconds > _PERIOD_TO_UPDATE_UI_IN_MS)
                    {
                        this.MidPoint = (tobAsk.Price + tobBid.Price) / 2;
                        this.Spread = (tobAsk.Price - tobBid.Price) * _orderBook.SymbolMultiplier;
                        _BidTOB_SPLIT.RaiseUIThread();
                        _AskTOB_SPLIT.RaiseUIThread();
                    }
                }
                #endregion


                #region REAL TIME PRICES

                if (AskTOB != null && BidTOB != null)
                {
                    DateTime maxDate = Max(_realTimePrices.DefaultIfEmpty(new PlotInfoPriceChart()).Max(d => d.Date), Max(AskTOB.LocalTimeStamp, BidTOB.LocalTimeStamp));
                    var objToAdd = new PlotInfoPriceChart() { Date = maxDate, MidPrice = MidPoint, AskPrice = AskTOB.Price, BidPrice = BidTOB.Price, Volume = AskTOB.Size + BidTOB.Size };

                    objToAdd.BuyActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Buy).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderByDescending(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                    objToAdd.SellActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Sell).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderBy(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                    objToAdd.BuyActiveOrder = objToAdd.BuyActiveOrder == 0 ? null : objToAdd.BuyActiveOrder;
                    objToAdd.SellActiveOrder = objToAdd.SellActiveOrder == 0 ? null : objToAdd.SellActiveOrder;

                    _realTimePrices.Add(objToAdd);
                }
                #endregion


                #region RealTime OrderBook Sizes (or orders) [SCATTER BUBBLES]                
                _maxOrderSize = _orderBook.GetMaxOrderSize() * 2.0;
                double maxBubleSize = 30;

                if (_orderBook.Bids != null)
                {
                    foreach (var b in _orderBook.Bids)
                    {
                        _realTimeOrderBookBids.Add(new OrderBookLevel() { Date = b.LocalTimeStamp, Price = b.Price, Size = maxBubleSize * (b.Size / _maxOrderSize) });
                    }

                }
                if (_orderBook.Asks != null)
                {
                    foreach (var b in _orderBook.Asks)
                    {
                        _realTimeOrderBookAsks.Add(new OrderBookLevel() { Date = b.LocalTimeStamp, Price = b.Price, Size = maxBubleSize * (b.Size / _maxOrderSize) });
                    }
                }
                #endregion

                #region REMOVE OLD
                if (_realTimePrices.Count > 0)
                {
                    var minDate = _realTimePrices.Last().Date.AddSeconds(-REALTIME_ITEM_POINTS_IN_SEC);// _realTimePrices[0].Date;
                    var colToDeleteBids = _realTimeOrderBookBids.Where(x => x.Date < minDate).ToList();
                    var colToDeleteAsks = _realTimeOrderBookAsks.Where(x => x.Date < minDate).ToList();
                    var colToDelete = _realTimePrices.Where(x => x.Date < minDate).ToList();

                    //_realTimePrices.RemoveAt(0);
                    foreach (var toDelete in colToDelete)
                        _realTimePrices.Remove(toDelete);
                    foreach (var toDelete in colToDeleteBids)
                        _realTimeOrderBookBids.Remove(toDelete);
                    foreach (var toDelete in colToDeleteAsks)
                        _realTimeOrderBookAsks.Remove(toDelete);
                }
                #endregion



                #region REAL TIME SPREADS
                if (_realTimeSpread.Count > 0)
                {
                    var minDate = _realTimeSpread.Last().Date.AddSeconds(-REALTIME_ITEM_POINTS_IN_SEC);
                    var colToDelete = _realTimeSpread.Where(x => x.Date <= minDate).ToList();
                    foreach (var col in colToDelete)
                        _realTimeSpread.Remove(col);
                }
                if (AskTOB != null && BidTOB != null)
                {
                    DateTime maxDate = Max(AskTOB.LocalTimeStamp, BidTOB.LocalTimeStamp);
                    _realTimeSpread.Add(new PlotInfoPriceChart() { Date = maxDate, MidPrice = Spread });
                }
                #endregion




                if (DateTime.Now.Subtract(_LAST_UI_UPDATED).TotalMilliseconds > _PERIOD_TO_UPDATE_UI_IN_MS)
                    _LAST_UI_UPDATED = DateTime.Now;


            }
        }
        private void PROVIDERS_OnDataReceived(object sender, ProviderVM e)
        {
            if (_providers == null)
            {
                _providers = new ObservableCollection<ProviderVM>();
                RaisePropertyChanged(nameof(Providers));
            }
            if (!_providers.Any(x => x.ProviderName == e.ProviderName))
            {
                var cleanProvider = new ProviderVM();
                cleanProvider.ProviderName = e.ProviderName;
                cleanProvider.ProviderID = e.ProviderID;
                _providers.Add(cleanProvider);
            }
        }
        private DateTime Max(DateTime a, DateTime b)
        {
            return a > b ? a : b;
        }
        public OrderBook OrderBook
        {
            get => _orderBook;
            set => SetProperty(ref _orderBook, value);
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());
            
        }
        public ProviderVM SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => this.OrderBook = null);
        }
        public string SelectedLayer
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value, onChanged: () => this.OrderBook = null);
        }
        public ObservableCollection<PlotInfoPriceChart> RealTimePrices => _realTimePrices;
        public ObservableCollection<PlotInfoPriceChart> RealTimeSpread => _realTimeSpread;
        public ObservableCollection<OrderBookLevel> RealTimeOrderBookBids => _realTimeOrderBookBids;
        public ObservableCollection<OrderBookLevel> RealTimeOrderBookAsks => _realTimeOrderBookAsks;
        public ObservableCollection<ProviderVM> Providers => _providers;

        public BookItemPriceSplit BidTOB_SPLIT
        {
            get => _BidTOB_SPLIT;
            set => SetProperty(ref _BidTOB_SPLIT, value);
        }
        public BookItemPriceSplit AskTOB_SPLIT
        {
            get => _AskTOB_SPLIT;
            set => SetProperty(ref _AskTOB_SPLIT, value);
        }
        public double MidPoint
        {
            get => _MidPoint;
            set => SetProperty(ref _MidPoint, value);
        }
        public BookItem AskTOB
        {
            get => _AskTOB;
            set => SetProperty(ref _AskTOB, value);
        }
        public BookItem BidTOB
        {
            get => _BidTOB;
            set => SetProperty(ref _BidTOB, value);
        }
        public double Spread
        {
            get => _Spread;
            set => SetProperty(ref _Spread, value);
        }
        public double ChartPercentageWidth
        {
            get => _PercentageWidth;
            set => SetProperty(ref _PercentageWidth, value);
        }
        public double ChartMaximumValue_Y
        {
            get => _ChartMaximumValue_Y;
            set => SetProperty(ref _ChartMaximumValue_Y, value);
        }
        public double ChartMinimumValue_Y
        {
            get => _ChartMinimumValue_Y;
            set => SetProperty(ref _ChartMinimumValue_Y, value);
        }
        public ObservableCollectionEx<BookItem> AskCummulative
        {
            get
            {
                var ret = _orderBook?.AskCummulative;
                if (ret != null)
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.DefaultIfEmpty(new BookItem()).Max(x => x.Size));
                }
                return ret;
            }
        }
        public ObservableCollectionEx<BookItem> BidCummulative
        {
            get
            {
                var ret = _orderBook?.BidCummulative;
                if (ret != null && ret.Any())
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.Max(x => x.Size));
                }
                return ret;
            }
        }
        public ObservableCollectionEx<BookItem> Asks => _orderBook?.Asks;
        public ObservableCollectionEx<BookItem> Bids => _orderBook?.Bids;
    }
}
