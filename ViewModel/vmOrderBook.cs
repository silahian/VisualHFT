using VisualHFT.Extensions;
using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Mvvm;
using System.Windows.Threading;

namespace VisualHFT.ViewModel
{
    public class vmOrderBook : BindableBase
    {
        private OrderBook _orderBook;
        protected object MTX_ORDERBOOK = new object();
        private Dictionary<string, Func<string, string, bool>> _dialogs;

        private ObservableCollection<PlotInfoPriceChart> _realTimePrices;
        ObservableCollection<PlotInfoPriceChart> _realTimeSpread;

        private ObservableCollection<ProviderVM> _providers;
        private string _selectedSymbol;
        private ProviderVM _selectedProvider = null;
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
        private Tuple<double, double> _bidBubbleSeriesVerticalAxis;


        private DispatcherTimer timerUI = new DispatcherTimer();

        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;


            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;

            timerUI.Interval = TimeSpan.FromMilliseconds(300);
            timerUI.Tick += TimerUI_Tick;
            timerUI.Start();

        }
        public vmOrderBook(vmOrderBook vm)
        {
            this._providers = vm._providers;
            this._dialogs = vm._dialogs;
            this._AskTOB = vm.AskTOB;
            this._AskTOB_SPLIT = vm.AskTOB_SPLIT;
            this._BidTOB = vm.BidTOB;
            this._BidTOB_SPLIT = vm.BidTOB_SPLIT;
            this._ChartMaximumValue_Y = vm.ChartMaximumValue_Y;
            this._ChartMinimumValue_Y = vm.ChartMinimumValue_Y;
            this._orderBook =  vm.OrderBook;
            this._realTimePrices = vm.RealTimePrices;
            this._realTimeSpread = vm.RealTimeSpread;
            this._selectedSymbol = vm.SelectedSymbol;
            this._selectedProvider = vm.SelectedProvider;
            this._layerName = vm.SelectedLayer;
            this._MidPoint = vm.MidPoint;
            this._bidBubbleSeriesVerticalAxis = vm.BidBubbleSeriesVerticalAxis;
        }
        private void TimerUI_Tick(object sender, EventArgs e)
        {

            _BidTOB_SPLIT?.RaiseUIThread();
            _AskTOB_SPLIT?.RaiseUIThread();

            RaisePropertyChanged(nameof(Bids));
            RaisePropertyChanged(nameof(Asks));
            RaisePropertyChanged(nameof(ChartMaximumValue_Y));


            RaisePropertyChanged(nameof(AskCummulative));
            RaisePropertyChanged(nameof(BidCummulative));
            RaisePropertyChanged(nameof(RealTimePrices));
            RaisePropertyChanged(nameof(RealTimeOrderLevelsAsk));
            RaisePropertyChanged(nameof(RealTimeOrderLevelsBid));
            RaisePropertyChanged(nameof(RealTimeSpread));
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
                var o = _orderBook.Asks.Where(x => x.Price.HasValue && Math.Abs(x.Price.Value - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _orderBook.Bids.Where(x => x.Price.HasValue && Math.Abs(x.Price.Value - e.PricePlaced) < comp).FirstOrDefault();

                if (o != null)
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

                var o = _orderBook.Asks.Where(x => x.Price.HasValue && Math.Abs(x.Price.Value - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _orderBook.Bids.Where(x => x.Price.HasValue && Math.Abs(x.Price.Value - e.PricePlaced) < comp).FirstOrDefault();

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

            lock (MTX_ORDERBOOK)
            {
                if (_orderBook == null || _orderBook.ProviderID != e.ProviderID || _orderBook.Symbol != e.Symbol)
                {
                    _maxOrderSize = 0; //reset
                    _orderBook = e;
                    _realTimePrices = new ObservableCollection<PlotInfoPriceChart>();
                    //_realTimePrices.CollectionChanged += (sender2, e2) => UpdateBubbleAxisRange();

                    _realTimeSpread = new ObservableCollection<PlotInfoPriceChart>();

                    _AskTOB_SPLIT = new BookItemPriceSplit();
                    _BidTOB_SPLIT = new BookItemPriceSplit();
                    RaisePropertyChanged(nameof(BidTOB_SPLIT));
                    RaisePropertyChanged(nameof(AskTOB_SPLIT));


                }
                if (!_orderBook.LoadData(e.Asks?.ToList(), e.Bids?.ToList()))
                    return; //if nothing to update, then exit


                #region Calculate TOB values
                var tobBid = _orderBook?.GetTOB(true);
                var tobAsk = _orderBook?.GetTOB(false);

                if (tobAsk != null && (tobAsk.Price != _AskTOB.Price || tobAsk.Size != _AskTOB.Size))
                {
                    this.AskTOB = tobAsk;
                    if (tobAsk.Price.HasValue && tobAsk.Size.HasValue)
                        _AskTOB_SPLIT.SetNumber(tobAsk.Price.Value, tobAsk.Size.Value, _orderBook.DecimalPlaces);
                }
                if (tobBid != null && (tobBid.Price != _BidTOB.Price || tobBid.Size != _BidTOB.Size))
                {
                    this.BidTOB = tobBid;
                    if (tobBid.Price.HasValue && tobBid.Size.HasValue)
                        _BidTOB_SPLIT.SetNumber(tobBid.Price.Value, tobBid.Size.Value, _orderBook.DecimalPlaces);
                }
                if (tobAsk != null && tobBid != null)
                {
                    if (tobBid.Price.HasValue && tobAsk.Price.HasValue)
                    {
                        this.MidPoint = (tobAsk.Price.Value + tobBid.Price.Value) / 2;
                        this.Spread = (tobAsk.Price.Value - tobBid.Price.Value) * _orderBook.SymbolMultiplier;
                    }
                }
                #endregion


                #region REAL TIME PRICES

                if (_realTimePrices != null && tobAsk != null && tobBid != null)
                {
                    lock (_realTimePrices)
                    {
                        DateTime maxDateIncoming = Max(tobAsk.LocalTimeStamp, tobBid.LocalTimeStamp);
                        var lastPoint = _realTimePrices.DefaultIfEmpty(new PlotInfoPriceChart()).LastOrDefault().Date;
                        if (maxDateIncoming.Subtract(lastPoint).TotalMilliseconds > 100) //taking samples every 100ms
                        {
                            var objToAdd = new PlotInfoPriceChart() { Date = maxDateIncoming, MidPrice = MidPoint, AskPrice = tobAsk.Price.Value, BidPrice = tobBid.Price.Value, Volume = tobAsk.Size.Value + tobBid.Size.Value };
                            objToAdd.BuyActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Buy).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderByDescending(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                            objToAdd.SellActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Sell).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderBy(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                            objToAdd.BuyActiveOrder = objToAdd.BuyActiveOrder == 0 ? null : objToAdd.BuyActiveOrder;
                            objToAdd.SellActiveOrder = objToAdd.SellActiveOrder == 0 ? null : objToAdd.SellActiveOrder;


                            #region Resting Orders at different levels [SCATTER BUBBLES]
                            double maxBubleSize = 30;
                            _maxOrderSize = _orderBook.GetMaxOrderSize() * 2.0;
                            if (_orderBook.Bids != null && _orderBook.Bids.Any())
                            {
                                objToAdd.BidOrders = new List<OrderBookLevel>();
                                foreach (var b in _orderBook.Bids.Where(x => x.Price.HasValue && x.Size.HasValue))
                                    objToAdd.BidOrders.Add(new OrderBookLevel() { Date = objToAdd.Date, Price = b.Price.Value, Size = maxBubleSize * (b.Size.Value / _maxOrderSize) });
                            }
                            if (_orderBook.Asks != null && _orderBook.Asks.Any())
                            {
                                objToAdd.AskOrders = new List<OrderBookLevel>();
                                foreach (var b in _orderBook.Asks.Where(x => x.Price.HasValue && x.Size.HasValue))
                                    objToAdd.AskOrders.Add(new OrderBookLevel() { Date = objToAdd.Date, Price = b.Price.Value, Size = maxBubleSize * (b.Size.Value / _maxOrderSize) });
                            }

                            double percentageToCut = 0.10 / 100.0;
                            objToAdd.BidOrders.RemoveAll(x => x.Price < objToAdd.MidPrice * (1.0 - percentageToCut));
                            objToAdd.AskOrders.RemoveAll(x => x.Price > objToAdd.MidPrice * (1.0 + percentageToCut));

                            #endregion
                            _realTimePrices.Add(objToAdd);
                            if (_realTimePrices.Count > 100) //max chart points = 100
                                _realTimePrices.RemoveAt(0);
                        }
                    }
                }
                #endregion


                #region REAL TIME SPREADS
                if (_realTimeSpread != null)
                {
                    lock (_realTimeSpread)
                    {
                        var lastAddedFromPrice = _realTimePrices.LastOrDefault();
                        var lastSpread = _realTimeSpread.LastOrDefault();
                        if (AskTOB != null && BidTOB != null && lastAddedFromPrice != null)
                        {
                            if (lastSpread == null || lastSpread.Date != lastAddedFromPrice.Date)
                                _realTimeSpread.Add(new PlotInfoPriceChart() { Date = lastAddedFromPrice.Date, MidPrice = Spread });

                            if (_realTimeSpread.Count > 100) //max chart points = 100
                                _realTimeSpread.RemoveAt(0);
                        }
                    }
                }
                #endregion


            }
        }
        private void UpdateBubbleAxisRange()
        {
            // Calculate the minimum and maximum Y-values in RealTimePrices
            double minY = RealTimePrices.Min(item => item.MidPrice);
            double maxY = RealTimePrices.Max(item => item.MidPrice);

            // Update the Minimum and Maximum of the BubbleSeries's Y-axis
            _bidBubbleSeriesVerticalAxis = new Tuple<double, double>(minY, maxY);

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
        public ObservableCollection<PlotInfoPriceChart> RealTimePrices
        {
            get
            {
                if (_realTimePrices == null)
                    return null;
                lock (_realTimePrices) 
                    return new ObservableCollection<PlotInfoPriceChart>(_realTimePrices);
            }
        } 
        public ObservableCollection<OrderBookLevel> RealTimeOrderLevelsAsk
        {
            get
            {
                if (_realTimePrices == null)
                    return null;
                lock (_realTimePrices)
                {                    
                    return new ObservableCollection<OrderBookLevel>(_realTimePrices.SelectMany(x => x.AskOrders));
                }
            }
        }
        public ObservableCollection<OrderBookLevel> RealTimeOrderLevelsBid
        {
            get
            {
                if (_realTimePrices == null)
                    return null;
                lock (_realTimePrices)
                {
                    return new ObservableCollection<OrderBookLevel>(_realTimePrices.SelectMany(x => x.BidOrders));
                }
            }
        }
        public ObservableCollection<PlotInfoPriceChart> RealTimeSpread
        {
            get
            {
                if (_realTimeSpread == null)
                    return null;
                lock(_realTimeSpread)
                    return new ObservableCollection<PlotInfoPriceChart>(_realTimeSpread);
            }
        } 
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
        public List<BookItem> AskCummulative
        {
            get
            {
                var ret = _orderBook?.AskCummulative;
                if (ret != null)
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.DefaultIfEmpty(new BookItem()).Max(x => x.Size.Value));
                }
                return ret;
            }
        }
        public List<BookItem> BidCummulative
        {
            get
            {
                var ret = _orderBook?.BidCummulative;
                if (ret != null && ret.Any())
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.Max(x => x.Size.Value));
                }
                return ret;
            }
        }
        public List<BookItem> Asks => _orderBook?.Asks;
        public List<BookItem> Bids => _orderBook?.Bids;
        public Tuple<double, double> BidBubbleSeriesVerticalAxis { get => _bidBubbleSeriesVerticalAxis; }

    }
}
