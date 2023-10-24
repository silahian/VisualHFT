using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using VisualHFT.Helpers;
using VisualHFT.Model;
using VisualHFT.ViewModel.Model;

namespace VisualHFT.ViewModel
{
    public class vmOrderBook : BindableBase, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private int _MAX_CHART_POINTS = 300;
        private AggregationLevel _AGG_LEVEL_CHARTS = AggregationLevel.Ms100;

        private OrderBook _orderBook;
        protected object MTX_ORDERBOOK = new object();

        private Dictionary<string, Func<string, string, bool>> _dialogs;

        private AggregatedCollection<PlotInfoPriceChart> _realTimePrices;
        private IEnumerable<OrderBookLevel> _realTimeOrderLevelsAsk;
        private IEnumerable<OrderBookLevel> _realTimeOrderLevelsBid;
        private AggregatedCollection<PlotInfoPriceChart> _realTimeSpread;
        private ObservableCollection<VisualHFT.ViewModel.Model.Trade> _realTimeTrades;
        private ObservableCollection<BookItem> _bidsGrid;
        private ObservableCollection<BookItem> _asksGrid;
        private ObservableCollection<BookItem> _depthGrid;

        private ObservableCollection<VisualHFT.ViewModel.Model.Provider> _providers;
        private string _selectedSymbol;
        private VisualHFT.ViewModel.Model.Provider _selectedProvider = null;
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
        private double _depthChartMaxY = 0;


        private UIUpdater uiUpdater;


        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;

            _bidsGrid = new ObservableCollection<BookItem>();
            _asksGrid = new ObservableCollection<BookItem>();
            _depthGrid = new ObservableCollection<BookItem>();
            Asks = CollectionViewSource.GetDefaultView(_asksGrid);
            Bids = CollectionViewSource.GetDefaultView(_bidsGrid);
            Depth = CollectionViewSource.GetDefaultView(_depthGrid);
            SetSortDescriptions();


            HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            HelperCommon.PROVIDERS.OnHeartBeatFail += PROVIDERS_OnHeartBeatFail;
            HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;
            HelperCommon.TRADES.OnDataReceived += TRADES_OnDataReceived;

            //EventAggregator.Instance.OnOrderBookDataReceived += LIMITORDERBOOK_OnDataReceived;
            HelperOrderBook.Instance.Subscribe(LIMITORDERBOOK_OnDataReceived);


            uiUpdater = new UIUpdater(uiUpdaterAction, 200);
            _providers = HelperCommon.PROVIDERS.CreateObservableCollection();
            RaisePropertyChanged(nameof(Providers));


            BidTOB_SPLIT = new BookItemPriceSplit();
            AskTOB_SPLIT = new BookItemPriceSplit();
            RaisePropertyChanged(nameof(BidTOB_SPLIT));
            RaisePropertyChanged(nameof(AskTOB_SPLIT));

        }
        public void SetSortDescriptions()
        {
            Asks.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
            Bids.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
            Depth.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
        }
        ~vmOrderBook()
        {
            Dispose(false);
        }
        private void uiUpdaterAction()
        {
            lock (MTX_ORDERBOOK)
            {
                _AskTOB_SPLIT?.RaiseUIThread();
                _BidTOB_SPLIT?.RaiseUIThread();

                RaisePropertyChanged(nameof(MidPoint));
                RaisePropertyChanged(nameof(Spread));

                BidAskGridUpdate();

                RaisePropertyChanged(nameof(AskCummulative));
                RaisePropertyChanged(nameof(BidCummulative));

                CalculateMaximumCummulativeSizeOnBothSides();
                RaisePropertyChanged(nameof(DepthChartMaxY));


                RaisePropertyChanged(nameof(RealTimeOrderLevelsAsk));
                RaisePropertyChanged(nameof(RealTimeOrderLevelsBid));

                RaisePropertyChanged(nameof(RealTimePrices));
                RaisePropertyChanged(nameof(RealTimeSpread));


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

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _bidsGrid.Clear();
                    _asksGrid.Clear();
                }));

                _AskTOB_SPLIT.Clear();
                _BidTOB_SPLIT.Clear();
                _orderBook = new OrderBook();
                _realTimePrices = new AggregatedCollection<PlotInfoPriceChart>(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS, x => x.Date, _realTimePrices_OnAggregate);
                _realTimeSpread = new AggregatedCollection<PlotInfoPriceChart>(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS, x => x.Date, _realTimePrices_OnAggregate);
                _realTimeTrades = new ObservableCollection<VisualHFT.ViewModel.Model.Trade>();
                _realTimeOrderLevelsAsk = new List<OrderBookLevel>();
                _realTimeOrderLevelsBid = new List<OrderBookLevel>();
                _maxOrderSize = 0; //reset
                _minOrderSize = 0; //reset
                _realTimeYAxisMaximum = 0;
                _realTimeYAxisMinimum = 0;
                _depthChartMaxY = 0;

                RaisePropertyChanged(nameof(AskCummulative));
                RaisePropertyChanged(nameof(BidCummulative));
                RaisePropertyChanged(nameof(Asks));
                RaisePropertyChanged(nameof(Bids));
                RaisePropertyChanged(nameof(RealTimePrices));
                RaisePropertyChanged(nameof(RealTimeSpread));
                RaisePropertyChanged(nameof(Trades));
            }
        }
        private void _realTimePrices_OnAggregate(PlotInfoPriceChart existing, PlotInfoPriceChart newItem)
        {
            // Update the existing bucket with the new values
            existing.Volume = newItem.Volume;
            existing.MidPrice = newItem.MidPrice;
            existing.BidPrice = newItem.BidPrice;
            existing.AskPrice = newItem.AskPrice;
            existing.BuyActiveOrder = newItem.BuyActiveOrder;
            existing.SellActiveOrder = newItem.SellActiveOrder;
        }
        private void ACTIVEORDERS_OnDataRemoved(object sender, VisualHFT.Model.Order e)
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
        private void ACTIVEORDERS_OnDataReceived(object sender, VisualHFT.Model.Order e)
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
        private void LIMITORDERBOOK_OnDataReceived(OrderBook e)
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

                    _realTimePrices = new AggregatedCollection<PlotInfoPriceChart>(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS, x => x.Date, _realTimePrices_OnAggregate);
                    _realTimeSpread = new AggregatedCollection<PlotInfoPriceChart>(_AGG_LEVEL_CHARTS, _MAX_CHART_POINTS, x => x.Date, _realTimePrices_OnAggregate);
                    _realTimeOrderLevelsAsk = new List<OrderBookLevel>();
                    _realTimeOrderLevelsBid = new List<OrderBookLevel>();
                    _AskTOB_SPLIT.Clear();
                    _BidTOB_SPLIT.Clear();
                }

                if (!_orderBook.LoadData(e.Asks, e.Bids))
                    return; //if nothing to update, then exit


                #region Calculate TOB values
                var tobBid = _orderBook?.GetTOB(true);
                var tobAsk = _orderBook?.GetTOB(false);
                _MidPoint = _orderBook != null ? _orderBook.MidPrice : 0;
                _Spread = _orderBook != null ? _orderBook.Spread : 0;

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
                    DateTime maxDateIncoming = DateTime.Now;// Max(tobAsk.LocalTimeStamp, tobBid.LocalTimeStamp);
                    var objToAdd = new PlotInfoPriceChart()
                    {
                        Date = maxDateIncoming,
                        MidPrice = MidPoint,
                        AskPrice = tobAsk.Price.Value,
                        BidPrice = tobBid.Price.Value,
                        Volume = tobAsk.Size.Value + tobBid.Size.Value
                    };

                    if (HelperCommon.ACTIVEORDERS.Any(x => x.Value.ProviderId == _selectedProvider.ProviderCode && x.Value.Symbol == _selectedSymbol))
                    {
                        objToAdd.BuyActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Buy).Select(x => x.Value).DefaultIfEmpty(new VisualHFT.Model.Order()).OrderByDescending(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                        objToAdd.SellActiveOrder = HelperCommon.ACTIVEORDERS.Where(x => x.Value.Side == eORDERSIDE.Sell).Select(x => x.Value).DefaultIfEmpty(new VisualHFT.Model.Order()).OrderBy(x => x.PricePlaced).FirstOrDefault().PricePlaced;
                        objToAdd.BuyActiveOrder = objToAdd.BuyActiveOrder == 0 ? null : objToAdd.BuyActiveOrder;
                        objToAdd.SellActiveOrder = objToAdd.SellActiveOrder == 0 ? null : objToAdd.SellActiveOrder;
                    }

                    #region Resting Orders at different levels [SCATTER BUBBLES]
                    var sizeMinMax = _orderBook.GetMinMaxSizes();
                    _minOrderSize = Math.Min(sizeMinMax.Item1, _minOrderSize);
                    _maxOrderSize = Math.Max(sizeMinMax.Item2, _maxOrderSize);

                    double minBubbleSize = 1; // Minimum size for bubbles in pixels
                    double maxBubbleSize = 10; // Maximum size for bubbles in pixels
                    if (_realTimePrices.Count() >= _MAX_CHART_POINTS - 10)
                    {
                        if (_orderBook.Bids != null)
                        {
                            foreach (var bid in _orderBook.Bids)
                            {
                                if (bid.Price.HasValue && bid.Size.HasValue)
                                {
                                    objToAdd.BidLevelOrders.Add(new OrderBookLevel()
                                    {
                                        Date = objToAdd.Date,
                                        Price = bid.Price.Value,
                                        Size = minBubbleSize + (bid.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize)
                                    });
                                }
                            }
                        }
                        if (_orderBook.Asks != null)
                        {
                            foreach (var ask in _orderBook.Asks)
                            {
                                if (ask.Price.HasValue && ask.Size.HasValue)
                                {
                                    objToAdd.AskLevelOrders.Add(new OrderBookLevel()
                                    {
                                        Date = objToAdd.Date,
                                        Price = ask.Price.Value,
                                        Size = minBubbleSize + (ask.Size.Value - _minOrderSize) / (_maxOrderSize - _minOrderSize) * (maxBubbleSize - minBubbleSize)
                                    });
                                }
                            }
                        }
                    }

                    #endregion
                    _realTimePrices.Add(objToAdd);
                    _realTimeOrderLevelsAsk = _realTimePrices.SelectMany(x => x.AskLevelOrders);
                    _realTimeOrderLevelsBid = _realTimePrices.SelectMany(x => x.BidLevelOrders);


                    //calculate min/max axis
                    _realTimeYAxisMinimum = _realTimePrices.Min(x => Math.Min(x.AskPrice, x.BidPrice)) * 0.9999; // midPrice * 0.9; //-20%
                    _realTimeYAxisMaximum = _realTimePrices.Max(x => Math.Max(x.AskPrice, x.BidPrice)) * 1.0001; // midPrice * 1.1; //+20%
                }
                #endregion

                #region REAL TIME SPREADS
                if (_realTimeSpread != null)
                {
                    PlotInfoPriceChart lastAddedFromPrice = _realTimePrices.LastOrDefault();
                    PlotInfoPriceChart lastSpread = _realTimeSpread.LastOrDefault();
                    if (AskTOB != null && BidTOB != null && lastAddedFromPrice != null)
                    {
                        if (lastSpread == null || lastSpread.Date != lastAddedFromPrice.Date)
                            _realTimeSpread.Add(new PlotInfoPriceChart() { Date = lastAddedFromPrice.Date, MidPrice = Spread });
                    }
                }
                #endregion
            }
        }
        private void BidAskGridUpdate()
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol))
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (_bidsGrid == null && _orderBook != null && _orderBook.Bids != null)
            {
                _bidsGrid.Clear();
                RaisePropertyChanged(nameof(Bids));
            }
            else if (_orderBook != null && _orderBook.Bids != null)
            {
                _orderBook.GetAddDeleteUpdate(ref _bidsGrid, _orderBook.Bids);
            }

            if (_asksGrid == null && _orderBook != null && _orderBook.Asks != null)
            {
                _asksGrid.Clear();
                RaisePropertyChanged(nameof(Asks));
            }
            else if (_orderBook != null && _orderBook.Asks != null)
            {
                _orderBook.GetAddDeleteUpdate(ref _asksGrid, _orderBook.Asks);
            }
            if (_asksGrid != null && _bidsGrid != null)
            {
                _depthGrid.Clear();
                foreach (var item in _asksGrid)
                    _depthGrid.Add(item);
                foreach (var item in _bidsGrid)
                    _depthGrid.Add(item);
            }
        }
        private void TRADES_OnDataReceived(object sender, VisualHFT.ViewModel.Model.Trade e)
        {
            if (e == null)
                return;
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderCode != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --" || _selectedSymbol != e.Symbol)
                return;


            if (_realTimeTrades != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    _realTimeTrades.Insert(0, e);
                    while (_realTimeTrades.Count > 100)
                        _realTimeTrades.RemoveAt(_realTimeTrades.Count - 1);
                }));
            }
        }
        private void PROVIDERS_OnDataReceived(object sender, VisualHFT.ViewModel.Model.Provider e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                _providers.Add(e);
                //if nothing is selected
                if (_selectedProvider == null) //default provider must be the first who's Active
                    SelectedProvider = e;
            }));
        }
        private void PROVIDERS_OnHeartBeatFail(object sender, VisualHFT.ViewModel.Model.Provider e)
        {
            if (_selectedProvider != null && e.ProviderCode == _selectedProvider.ProviderCode && (e.Status == eSESSIONSTATUS.PRICE_DSICONNECTED_ORDER_CONNECTED || e.Status == eSESSIONSTATUS.BOTH_DISCONNECTED))
                Clear();
        }
        public OrderBook OrderBook
        {
            get { return _orderBook; }
        }
        public string SelectedSymbol
        {
            get => _selectedSymbol;
            set => SetProperty(ref _selectedSymbol, value, onChanged: () => Clear());

        }
        public VisualHFT.ViewModel.Model.Provider SelectedProvider
        {
            get => _selectedProvider;
            set => SetProperty(ref _selectedProvider, value, onChanged: () => Clear());
        }
        public string SelectedLayer
        {
            get => _layerName;
            set => SetProperty(ref _layerName, value, onChanged: () => Clear());
        }
        public IReadOnlyList<PlotInfoPriceChart> RealTimePrices => _realTimePrices?.AsReadOnly();
        public IEnumerable<OrderBookLevel> RealTimeOrderLevelsAsk => _realTimeOrderLevelsAsk;
        public IEnumerable<OrderBookLevel> RealTimeOrderLevelsBid => _realTimeOrderLevelsBid;
        public ReadOnlyCollection<PlotInfoPriceChart> RealTimeSpread => _realTimeSpread?.AsReadOnly();
        public ObservableCollection<VisualHFT.ViewModel.Model.Provider> Providers => _providers;
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
        private void CalculateMaximumCummulativeSizeOnBothSides()
        {
            double? _maxValueAsks = OrderBook?.AskCummulative?.DefaultIfEmpty(new BookItem() { Size = 0 }).Max(x => x.Size.Value);
            if (!_maxValueAsks.HasValue) _maxValueAsks = 0;
            double? _maxValueBids = OrderBook?.BidCummulative?.DefaultIfEmpty(new BookItem() { Size = 0 }).Max(x => x.Size.Value);
            if (!_maxValueBids.HasValue) _maxValueBids = 0;

            _depthChartMaxY = Math.Max(_maxValueBids.Value, _maxValueAsks.Value);
        }

        public ReadOnlyCollection<BookItem> AskCummulative => OrderBook?.AskCummulative;
        public ReadOnlyCollection<BookItem> BidCummulative => OrderBook?.BidCummulative;

        public ICollectionView Asks { get; }
        public ICollectionView Bids { get; }
        public ICollectionView Depth { get; }


        public ObservableCollection<VisualHFT.ViewModel.Model.Trade> Trades
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
        public double DepthChartMaxY
        {
            get => _depthChartMaxY;
        }

        public int SwitchView
        {
            get => _switchView;
            set => SetProperty(ref _switchView, value);
        }

        private int _switchView = 0;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    uiUpdater.Dispose();
                    HelperCommon.PROVIDERS.OnDataReceived -= PROVIDERS_OnDataReceived;
                    HelperCommon.PROVIDERS.OnHeartBeatFail -= PROVIDERS_OnHeartBeatFail;
                    HelperCommon.ACTIVEORDERS.OnDataReceived -= ACTIVEORDERS_OnDataReceived;
                    HelperCommon.ACTIVEORDERS.OnDataRemoved -= ACTIVEORDERS_OnDataRemoved;
                    HelperCommon.TRADES.OnDataReceived -= TRADES_OnDataReceived;
                    HelperOrderBook.Instance.Unsubscribe(LIMITORDERBOOK_OnDataReceived);

                    _orderBook?.Dispose();
                    _dialogs = null;
                    _realTimePrices?.Clear();
                    _realTimeSpread?.Clear();
                    _realTimeTrades?.Clear();
                    _providers?.Clear();

                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
