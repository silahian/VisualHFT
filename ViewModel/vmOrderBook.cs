using VisualHFT.Extensions;
using VisualHFT.Helpers;
using VisualHFT.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ChartView;

namespace VisualHFT.ViewModel
{
    public class vmOrderBook : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private OrderBook _OrderBook;
        private Dictionary<string, Func<string, string, bool>> _dialogs;
        private int REALTIME_ITEM_POINTS = 100;
        private ObservableCollection<PlotInfoPriceChart> _realTimePrices;
        ObservableCollection<PlotInfoPriceChart> _realTimeSpread;
        ObservableCollection<OrderBookLevel> _realTimeOrderBookBids;
        ObservableCollection<OrderBookLevel> _realTimeOrderBookAsks;

        private ObservableCollection<Provider> _providers;
        private string _selectedSymbol;
        private Provider _selectedProvider=null;
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

        public vmOrderBook(Dictionary<string, Func<string, string, bool>> dialogs)
        {
            this._dialogs = dialogs;


            Helpers.HelperCommon.PROVIDERS.OnDataReceived += PROVIDERS_OnDataReceived;
            Helpers.HelperCommon.LIMITORDERBOOK.OnDataReceived += LIMITORDERBOOK_OnDataReceived;
            Helpers.HelperCommon.ACTIVEORDERS.OnDataReceived += ACTIVEORDERS_OnDataReceived;
            Helpers.HelperCommon.ACTIVEORDERS.OnDataRemoved += ACTIVEORDERS_OnDataRemoved;

        }

        private void ACTIVEORDERS_OnDataRemoved(object sender, OrderVM e)
        {
            if (_selectedProvider == null || string.IsNullOrEmpty(_selectedSymbol) || _selectedProvider.ProviderID != e.ProviderId)
                return;
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (_OrderBook != null)
            {
                var comp = 1.0 / Math.Pow(10, e.SymbolDecimals);
                var o = _OrderBook.Asks.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _OrderBook.Bids.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();

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

            if (_OrderBook != null)
            {
                var comp = 1.0 / Math.Pow(10, e.SymbolDecimals);

                var o = _OrderBook.Asks.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();
                if (o == null)
                    o = _OrderBook.Bids.Where(x => Math.Abs(x.Price - e.PricePlaced) < comp).FirstOrDefault();

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
            if (string.IsNullOrEmpty(_selectedSymbol) || _selectedSymbol == "-- All symbols --")
                return;

            if (true)
            {
                if (_OrderBook == null || _OrderBook.ProviderID != e.ProviderID || _OrderBook.Symbol != e.Symbol)
                {
                    _maxOrderSize = 0; //reset
                    _OrderBook = e;
                    _realTimePrices = new ObservableCollection<PlotInfoPriceChart>();
                    _realTimeSpread = new ObservableCollection<PlotInfoPriceChart>();

                    _realTimeOrderBookBids = new ObservableCollection<OrderBookLevel>();
                    _realTimeOrderBookAsks = new ObservableCollection<OrderBookLevel>();

                    _AskTOB_SPLIT = new BookItemPriceSplit();
                    _BidTOB_SPLIT = new BookItemPriceSplit();
                    

                    RaisePropertyChanged("RealTimeSpread");
                    RaisePropertyChanged("RealTimePrices");
                    RaisePropertyChanged("RealTimeOrderBookBids");
                    RaisePropertyChanged("RealTimeOrderBookAsks");
                    RaisePropertyChanged("AskTOB_SPLIT");
                    RaisePropertyChanged("BidTOB_SPLIT");
                }
                _OrderBook.LoadData(e.Asks.ToList(), e.Bids.ToList());
                RaisePropertyChanged("Bids");
                RaisePropertyChanged("Asks");
                RaisePropertyChanged("ChartMaximumValue_Y");
                //RaisePropertyChanged("ChartMinimumValue_Y");
                RaisePropertyChanged("AskCummulative");
                RaisePropertyChanged("BidCummulative");


                #region Calculate TOB values
                var tobBid = _OrderBook.GetTOB(true);
                var tobAsk = _OrderBook.GetTOB(false);

                if (tobAsk != null && (tobAsk.Price != _AskTOB.Price || tobAsk.Size != _AskTOB.Size))
                {
                    this.AskTOB = tobAsk;
                    _AskTOB_SPLIT.SetNumber(tobAsk.Price, tobAsk.Size, _OrderBook.DecimalPlaces);
                }
                if (tobBid != null && (tobBid.Price != _BidTOB.Price || tobBid.Size != _BidTOB.Size))
                {
                    this.BidTOB = tobBid;
                    _BidTOB_SPLIT.SetNumber(tobBid.Price, tobBid.Size, _OrderBook.DecimalPlaces);
                }
                this.MidPoint = (tobAsk.Price + tobBid.Price) / 2;
                this.Spread = (tobAsk.Price - tobBid.Price) * _OrderBook.SymbolMultiplier;

                #endregion



                #region REAL TIME PRICES
                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    if (_realTimePrices.Count > REALTIME_ITEM_POINTS)
                    {
                        var minDate = _realTimePrices[0].Date;
                        var colToDeleteBids = _realTimeOrderBookBids.Where(x => x.Date <= minDate).ToList();
                        var colToDeleteAsks = _realTimeOrderBookAsks.Where(x => x.Date <= minDate).ToList();

                        _realTimePrices.RemoveAt(0);
                        foreach (var toDelete in colToDeleteBids)
                            _realTimeOrderBookBids.Remove(toDelete);
                        foreach (var toDelete in colToDeleteAsks)
                            _realTimeOrderBookAsks.Remove(toDelete);
                    }
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


                    #region RealTime OrderBook Sizes (or orders) [SCATTER BUBBLES]                
                    _maxOrderSize = _OrderBook.GetMaxOrderSize() * 2.0;
                    double maxBubleSize = 30;

                    if (_OrderBook.Bids != null)
                    {
                        foreach (var b in _OrderBook.Bids)
                        {
                            _realTimeOrderBookBids.Add(new OrderBookLevel() { Date = b.LocalTimeStamp, Price = b.Price, Size = maxBubleSize * (b.Size / _maxOrderSize) });
                        }

                    }
                    if (_OrderBook.Asks != null)
                    {
                        foreach (var b in _OrderBook.Asks)
                        {
                            _realTimeOrderBookAsks.Add(new OrderBookLevel() { Date = b.LocalTimeStamp, Price = b.Price, Size = maxBubleSize * (b.Size / _maxOrderSize) });
                        }
                    }
                    #endregion
                }));

                #endregion




                return;
                #region REAL TIME SPREADS
                if (_realTimeSpread.Count > REALTIME_ITEM_POINTS)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        _realTimeSpread.RemoveAt(0);
                    }));
                }
                if (AskTOB != null && BidTOB != null)
                {
                    DateTime maxDate = Max(AskTOB.LocalTimeStamp, BidTOB.LocalTimeStamp);
                    _realTimeSpread.Add(new PlotInfoPriceChart() { Date = maxDate, MidPrice = Spread });
                }
                #endregion

                
            }
        }
        private void PROVIDERS_OnDataReceived(object sender, Provider e)
        {
            if (_providers == null)
            {
                _providers = new ObservableCollection<Provider>();
                RaisePropertyChanged("Providers");
            }
            if (!_providers.Any(x => x.ProviderName == e.ProviderName))
            {
                var cleanProvider = new Provider();
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
            get
            {
                return _OrderBook;
            }
            set
            {
                if (_OrderBook != value)
                {
                    _OrderBook = value;
                    RaisePropertyChanged("OrderBook");
                }
            }
        }
        public string SelectedSymbol
        {
            get
            {
                return _selectedSymbol;
            }

            set
            {
                if (_selectedSymbol != value)
                {
                    _selectedSymbol = value;
                    this.OrderBook = null;
                    RaisePropertyChanged("SelectedSymbol");
                }
            }
        }
        public Provider SelectedProvider
        {
            get
            {
                return _selectedProvider;
            }

            set
            {
                if (_selectedProvider != value)
                {
                    _selectedProvider = value;
                    this.OrderBook = null;
                    RaisePropertyChanged("SelectedProvider");
                }
            }
        }
        public string SelectedLayer
        {
            get { return _layerName; }
            set
            {
                if (_layerName != value)
                {
                    _layerName = value;
                    this.OrderBook = null;
                    RaisePropertyChanged("SelectedLayer");
                }
            }
        }
        public ObservableCollection<PlotInfoPriceChart> RealTimePrices { get { return _realTimePrices; } }
        public ObservableCollection<PlotInfoPriceChart> RealTimeSpread { get { return _realTimeSpread; } }
        public ObservableCollection<OrderBookLevel> RealTimeOrderBookBids { get { return _realTimeOrderBookBids; } }
        public ObservableCollection<OrderBookLevel> RealTimeOrderBookAsks { get { return _realTimeOrderBookAsks; } }
        public ObservableCollection<Provider> Providers { get { return _providers; } }

        public BookItemPriceSplit BidTOB_SPLIT
        {
            get { return _BidTOB_SPLIT; }
            set { _BidTOB_SPLIT = value; }
        }
        public BookItemPriceSplit AskTOB_SPLIT
        {
            get { return _AskTOB_SPLIT; }
            set { _AskTOB_SPLIT = value; }
        }
        public double MidPoint
        {
            get => _MidPoint;
            set
            {
                if (value != _MidPoint)
                {
                    _MidPoint = value;
                    RaisePropertyChanged("MidPoint");
                }
            }
        }
        public BookItem AskTOB
        {
            get => _AskTOB;
            set
            {
                if (_AskTOB == null || !_AskTOB.Equals(value))
                {
                    _AskTOB = value;
                    RaisePropertyChanged("AskTOB");
                }
            }
        }
        public BookItem BidTOB
        {
            get => _BidTOB;
            set
            {
                if (_BidTOB == null || !_BidTOB.Equals(value))
                {
                    _BidTOB = value;
                    RaisePropertyChanged("BidTOB");
                }
            }
        }
        public double Spread
        {
            get => _Spread;
            set
            {
                if (_Spread != value)
                {
                    _Spread = value;
                    RaisePropertyChanged("Spread");
                }
            }
        }
        public double ChartPercentageWidth
        {
            get
            {
                return _PercentageWidth;
            }
            set
            {
                if (_PercentageWidth != value)
                {
                    _PercentageWidth = value;
                    RaisePropertyChanged("ChartPercentageWidth");
                }
            }
        }
        public double ChartMaximumValue_Y
        {
            get { return _ChartMaximumValue_Y; }
            set
            {
                if (_ChartMaximumValue_Y != value)
                {
                    _ChartMaximumValue_Y = value;
                    RaisePropertyChanged("ChartMaximumValue_Y");
                }
            }
        }
        public double ChartMinimumValue_Y
        {
            get { return _ChartMinimumValue_Y; }
            set
            {
                if (_ChartMinimumValue_Y != value)
                {
                    _ChartMinimumValue_Y = value;
                    RaisePropertyChanged("ChartMinimumValue_Y");
                }
            }
        }
        public ObservableCollectionEx<BookItem> AskCummulative
        {
            get
            {
                var ret = _OrderBook?.AskCummulative;
                if (ret != null)
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.Max(x => x.Size));
                }
                return ret;
            }
        }
        public ObservableCollectionEx<BookItem> BidCummulative
        {
            get
            {
                var ret = _OrderBook?.BidCummulative;
                if (ret != null)
                {
                    _ChartMaximumValue_Y = Math.Max(_ChartMaximumValue_Y, ret.Max(x => x.Size));
                }
                return ret;
            }
        }
        public ObservableCollectionEx<BookItem> Asks
        {
            get
            {
                return _OrderBook?.Asks;
            }
        }
        public ObservableCollectionEx<BookItem> Bids
        {
            get
            {
                return _OrderBook?.Bids;
            }
        }
    }
}
