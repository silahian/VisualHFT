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
using System.Windows.Navigation;
using OxyPlot;

namespace VisualHFT.ViewModel
{
    public class vmOrderBook : BindableBase
    {
        private int _MAX_CHART_POINTS = 300;
        private AggregationLevel _AGG_LEVEL_CHARTS = AggregationLevel.Ms100;

        private OrderBook _orderBook;
        protected object MTX_ORDERBOOK = new object();

        private Dictionary<string, Func<string, string, bool>> _dialogs;

        private HelperAggregatedPlotCollection _realTimePrices;
        private ObservableCollection<Trade> _realTimeTrades;
        private List<PlotInfoPriceChart> _realTimeSpread;

        private ObservableCollection<Provider> _providers;
        private string _selectedSymbol;
        private Provider _selectedProvider = null;
        private string _layerName;
        private double _maxOrderSize = 0;
        private double _minOrderSize = 0;

        private double _MidPoint;
        private BookItem _AskTOB = new BookItem();
        private BookItem _BidTOB = new BookItem();        
        private double _Spread;

        private BookItemPriceSplit _BidTOB_SPLIT = null;
        private BookItemPriceSplit _AskTOB_SPLIT = null;
        private double _PercentageWidth = 1;
        private double _realTimeYAxisMinimum = 0;
        private double _realTimeYAxisMaximum = 0;
        

        private DispatcherTimer timerUI = new DispatcherTimer();
        private List<BookItem> TRACK_Bids = new List<BookItem>();
        private List<BookItem> TRACK_Asks = new List<BookItem>();
        private List<BookItem> TRACK_BidCummulative = new List<BookItem>();
        private List<BookItem> TRACK_AskCummulative = new List<BookItem>();
        private List<PlotInfoPriceChart> TRACK_RealTimePrices = new List<PlotInfoPriceChart>();
        private List<OrderBookLevel> TRACK_RealTimeOrderLevelsAsk = new List<OrderBookLevel>();
        private List<OrderBookLevel> TRACK_RealTimeOrderLevelsBid = new List<OrderBookLevel>();
        private List<PlotInfoPriceChart> TRACK_RealTimeSpread = new List<PlotInfoPriceChart>();
        

        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;

            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.PROVIDERS.OnHeartBeatFail += PROVIDERS_OnHeartBeatFail;
            HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;
            timerUI.Interval = TimeSpan.FromMilliseconds(100);
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
            this._orderBook = vm.OrderBook;
            this._realTimePrices = new HelperAggregatedPlotCollection(vm.RealTimePrices, _AGG_LEVEL_CHARTS, _MAX_CHART_POINTS);
            this._realTimeTrades = new ObservableCollection<Trade>();
            this._realTimeSpread = vm.RealTimeSpread;
            this._selectedSymbol = vm.SelectedSymbol;
            this._selectedProvider = vm.SelectedProvider;
            this._layerName = vm.SelectedLayer;
            this._MidPoint = vm.MidPoint;            
        }
        private void TimerUI_Tick(object sender, EventArgs e)
        {
            lock (MTX_ORDERBOOK)
            {
                _AskTOB_SPLIT?.RaiseUIThread();
                _BidTOB_SPLIT?.RaiseUIThread();



                if (Bids != null && !TRACK_Bids.SequenceEqual(Bids))
                {
                    RaisePropertyChanged(nameof(Bids));
                    TRACK_Bids = Bids.ToList();
                }
                if (Asks != null && !TRACK_Asks.SequenceEqual(Asks))
                {
                    RaisePropertyChanged(nameof(Asks));
                    TRACK_Asks = Asks.ToList();
                }


                if (AskCummulative != null && !TRACK_AskCummulative.SequenceEqual(AskCummulative))
                {
                    RaisePropertyChanged(nameof(AskCummulative));
                    TRACK_AskCummulative = AskCummulative.ToList();
                }
                if (BidCummulative != null && !TRACK_BidCummulative.SequenceEqual(BidCummulative))
                {
                    RaisePropertyChanged(nameof(BidCummulative));
                    TRACK_BidCummulative = BidCummulative.ToList();
                }

                if (RealTimePrices != null && !TRACK_RealTimePrices.SequenceEqual(RealTimePrices))
                {
                    RaisePropertyChanged(nameof(RealTimePrices));
                    TRACK_RealTimePrices = RealTimePrices.ToList();
                }


                if (RealTimeOrderLevelsAsk != null && !TRACK_RealTimeOrderLevelsAsk.SequenceEqual(RealTimeOrderLevelsAsk))
                {
                    RaisePropertyChanged(nameof(RealTimeOrderLevelsAsk));
                    TRACK_RealTimeOrderLevelsAsk = RealTimeOrderLevelsAsk.ToList();
                }
                if (RealTimeOrderLevelsBid != null && !TRACK_RealTimeOrderLevelsBid.SequenceEqual(RealTimeOrderLevelsBid))
                {
                    RaisePropertyChanged(nameof(RealTimeOrderLevelsBid));
                    TRACK_RealTimeOrderLevelsBid = RealTimeOrderLevelsBid.ToList();
                }

                if (RealTimeSpread != null && !TRACK_RealTimeSpread.SequenceEqual(RealTimeSpread))
                {
                    RaisePropertyChanged(nameof(RealTimeSpread));
                    TRACK_RealTimeSpread = RealTimeSpread.ToList();
                }

                
                RaisePropertyChanged(nameof(LOBImbalanceValue));
                RaisePropertyChanged(nameof(RealTimeYAxisMinimum));
                RaisePropertyChanged(nameof(RealTimeYAxisMaximum));
                
            }
        }

        private void Clear()
        {
            lock (MTX_ORDERBOOK)
            {
                this.MidPoint = 0;
                this.AskTOB = new BookItem();
                this.BidTOB = new BookItem();
                this.Spread = 0;

                _AskTOB_SPLIT = new BookItemPriceSplit();
                _BidTOB_SPLIT = new BookItemPriceSplit();
                _orderBook = new OrderBook();
                _realTimePrices = new HelperAggregatedPlotCollection(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS);
                _realTimeSpread = new List<PlotInfoPriceChart>();
                _realTimeTrades = new ObservableCollection<Trade>();
                _maxOrderSize = 0; //reset
                _minOrderSize = 0; //reset
                _realTimeYAxisMaximum = 0;
                _realTimeYAxisMinimum = 0;
            }

            RaisePropertyChanged(nameof(BidTOB_SPLIT));
            RaisePropertyChanged(nameof(AskTOB_SPLIT));
            RaisePropertyChanged(nameof(AskCummulative));
            RaisePropertyChanged(nameof(BidCummulative));
            RaisePropertyChanged(nameof(Asks));
            RaisePropertyChanged(nameof(Bids));
            RaisePropertyChanged(nameof(RealTimePrices));
            RaisePropertyChanged(nameof(RealTimeSpread));
            RaisePropertyChanged(nameof(Trades));
        }
        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            lock (MTX_ORDERBOOK)
            {
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
        }
        private void ACTIVEORDERS_OnDataReceived(object sender, OrderVM e)
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;
            lock (MTX_ORDERBOOK)
            {
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
        }
        private void LIMITORDERBOOK_OnDataReceived(object sender, OrderBook e)
        {
            if (e == null)
                return;            
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderID)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;

            lock (MTX_ORDERBOOK)
            {
                if (_orderBook == null || _orderBook.ProviderID != e.ProviderID || _orderBook.Symbol != e.Symbol)
                {
                    _maxOrderSize = 0; //reset
                    _minOrderSize = 0; //reset
                    _orderBook = e;
                    _orderBook.DecimalPlaces = e.DecimalPlaces;
                    _orderBook.SymbolMultiplier = e.SymbolMultiplier;

                    _realTimePrices = new HelperAggregatedPlotCollection(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS);
                    _realTimeSpread = new List<PlotInfoPriceChart>();
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
                this.MidPoint = _orderBook != null ? _orderBook.MidPrice : 0;
                this.Spread = _orderBook != null ? _orderBook.Spread : 0;

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
                #endregion

                #region REAL TIME PRICES

                if (_realTimePrices != null && tobAsk != null && tobBid != null)
                {
                    DateTime maxDateIncoming = Max(tobAsk.LocalTimeStamp, tobBid.LocalTimeStamp);
                    if (true) 
                    {
                        var objToAdd = new PlotInfoPriceChart() { Date = maxDateIncoming, MidPrice = MidPoint, AskPrice = tobAsk.Price.Value, BidPrice = tobBid.Price.Value, Volume = tobAsk.Size.Value + tobBid.Size.Value };
                        objToAdd.BuyActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Buy).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderByDescending(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                        objToAdd.SellActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Sell).Select(x => x.Value).DefaultIfEmpty(new OrderVM()).OrderBy(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                        objToAdd.BuyActiveOrder = objToAdd.BuyActiveOrder == 0 ? null : objToAdd.BuyActiveOrder;
                        objToAdd.SellActiveOrder = objToAdd.SellActiveOrder == 0 ? null : objToAdd.SellActiveOrder;
                        #region Resting Orders at different levels [SCATTER BUBBLES]
                        var sizeMinMax = _orderBook.GetMinMaxSizes();
                        _minOrderSize = Math.Min(sizeMinMax.Item1, _minOrderSize);
                        _maxOrderSize = Math.Max(sizeMinMax.Item2, _maxOrderSize);

                        double minBubbleSize = 1; // Minimum size for bubbles in pixels
                        double maxBubbleSize = 10; // Maximum size for bubbles in pixels

                        if (_realTimePrices.Count() >= _MAX_CHART_POINTS-10) //START ADDING bubbles when full frame is collected
                        {
                            objToAdd.BidOrders = new List<OrderBookLevel>();
                            objToAdd.AskOrders = new List<OrderBookLevel>();
                            if (_orderBook.Bids != null && _orderBook.Bids.Any())
                            {
                                foreach (var item in _orderBook.Bids.Where(x => x.Price.HasValue && x.Size.HasValue))
                                {
                                    double normalizedSize = minBubbleSize + (item.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize);
                                    objToAdd.BidOrders.Add(new OrderBookLevel() { Date = objToAdd.Date, Price = item.Price.Value, Size = normalizedSize });
                                }
                            }
                            if (_orderBook.Asks != null && _orderBook.Asks.Any())
                            {
                                foreach (var item in _orderBook.Asks.Where(x => x.Price.HasValue && x.Size.HasValue))
                                {
                                    double normalizedSize = minBubbleSize + (item.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize);
                                    objToAdd.AskOrders.Add(new OrderBookLevel() { Date = objToAdd.Date, Price = item.Price.Value, Size = normalizedSize });
                                }
                            }
                        }
                        #endregion
                        _realTimePrices.Add(objToAdd);

                        //calculate min/max axis
                        var midPrice = _realTimePrices.GetAvgOfMidPrice();
                        _realTimeYAxisMinimum = _realTimePrices.GetMinOfMidPrice() * 0.9999; // midPrice * 0.9; //-20%
                        _realTimeYAxisMaximum = _realTimePrices.GetMaxOfMidPrice() * 1.0001; // midPrice * 1.1; //+20%

                    }
                }
                #endregion

                #region REAL TIME SPREADS
                if (_realTimeSpread != null)
                {
                    var lastAddedFromPrice = _realTimePrices.LastOrDefault();
                    var lastSpread = _realTimeSpread.LastOrDefault();
                    if (AskTOB != null && BidTOB != null && lastAddedFromPrice != null)
                    {
                        if (lastSpread == null || lastSpread.Date != lastAddedFromPrice.Date)
                            _realTimeSpread.Add(new PlotInfoPriceChart() { Date = lastAddedFromPrice.Date, MidPrice = Spread });

                        if (_realTimeSpread.Count > _MAX_CHART_POINTS) //max chart points
                            _realTimeSpread.RemoveAt(0);
                    }
                }
                #endregion

            }
        }
        private void TRADES_OnDataReceived(object sender, Trade e)
        {
            if (e == null)
                return;
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;

            
            if (_realTimeTrades != null)
                App.Current.Dispatcher.Invoke(() => {
                    _realTimeTrades.Insert(0, e);                    
                    while (_realTimeTrades.Count > 100)
                        _realTimeTrades.RemoveAt(_realTimeTrades.Count-1);
                });
        }

        private void PROVIDERS_OnDataReceived(object sender, Provider e)
        {
            if (_providers == null)
            {
                _providers = new ObservableCollection<Provider>();
                RaisePropertyChanged(nameof(Providers));
            }
            if (!_providers.Any(x => x.ProviderName == e.ProviderName))
            {
                var cleanProvider = new Provider();
                cleanProvider.ProviderName = e.ProviderName;
                cleanProvider.ProviderCode = e.ProviderCode;
                _providers.Add(cleanProvider);
            }
        }
        private void PROVIDERS_OnHeartBeatFail(object sender, ProviderEx e)
        {
            if (_selectedProvider != null && e.ProviderCode == _selectedProvider.ProviderCode)
                Clear();
        }

        private DateTime Max(DateTime a, DateTime b)
        {
            return a > b ? a : b;
        }
        public OrderBook OrderBook
        {
            get {return _orderBook;}
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());

        }
        public Provider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => Clear());
        }
        public string SelectedLayer
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value, onChanged: () => Clear());
        }
        public List<PlotInfoPriceChart> RealTimePrices
        {
            get { return _realTimePrices?.ToList().ToList();}
        }
        public List<OrderBookLevel> RealTimeOrderLevelsAsk
        {
            get
            {
                if (_realTimePrices == null)
                    return null;
                return RealTimePrices.Where(x => x.AskOrders != null).SelectMany(x => x.AskOrders).ToList();
            }
        }
        public List<OrderBookLevel> RealTimeOrderLevelsBid
        {
            get
            {
                if (_realTimePrices == null)
                    return null;
                return RealTimePrices.Where(x => x.BidOrders != null).SelectMany(x => x.BidOrders).ToList();
            }
        }
        public List<PlotInfoPriceChart> RealTimeSpread
        {
            get
            {
                if (_realTimeSpread == null)
                    return null;
                return _realTimeSpread?.ToList();                    
            }
        }
        public ObservableCollection<Provider> Providers => _providers;

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
        public double LOBImbalanceValue => OrderBook?.ImbalanceValue ?? 0;
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
        public List<BookItem> AskCummulative
        {
            get
            {
                var ret = OrderBook?.AskCummulative?.ToList();
                return ret;
            }
        }
        public List<BookItem> BidCummulative
        {
            get
            {
                var ret = OrderBook?.BidCummulative?.ToList();
                return ret;
            }
        }
        public List<BookItem> Asks => OrderBook?.Asks?.ToList();
        public List<BookItem> Bids => OrderBook?.Bids?.ToList();
        public ObservableCollection<Trade> Trades
        { 
            get => _realTimeTrades;
            set => SetProperty(ref _realTimeTrades, value);
        }
        public double RealTimeYAxisMinimum
        {
            get => _realTimeYAxisMinimum;
        }
        public double RealTimeYAxisMaximum
        {
            get => _realTimeYAxisMaximum;
        }

    }
}
