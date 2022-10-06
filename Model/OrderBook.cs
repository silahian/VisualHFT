using VisualHFT.Extensions;
using VisualHFT.Helpers;
using Microsoft.Expression.Interactivity.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ChartView;

namespace VisualHFT.Model
{
    public class OrderBook
    {
        private ObservableCollectionEx<BookItem> _Bids;
        private ObservableCollectionEx<BookItem> _Asks;

        private ObservableCollectionEx<BookItem> _Cummulative_Bids;
        private ObservableCollectionEx<BookItem> _Cummulative_Asks;

        protected object _bidLock = new object();
        protected object _askLock = new object();
        protected object _cumBidLock = new object();
        protected object _cumAskLock = new object();
        private string _KEY;
        private string _Symbol;
        private int _DecimalPlaces;
        private double _SymbolMultiplier;
        private int _ProviderID;
        private string _ProviderName;
        private eSESSIONSTATUS _providerStatus;


        private void SetKey()
        {
            if (this.ProviderID >= -1 && this.Symbol != "")
            {
                _KEY = this.ProviderID.ToString() + "_" + this.Symbol;
            }
        }
        private bool GetAddDeleteUpdate(ObservableCollectionEx<BookItem> inputExisting, List<BookItem> inputNew, out List<BookItem> outAdds, out List<BookItem> outUpdates, out List<BookItem> outRemoves)
        {
            outRemoves = inputExisting.Where(e => !inputNew.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outUpdates = inputNew.Where(e => inputExisting.Any(i => i.Size != e.Size && i.Price == e.Price && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outAdds = inputNew.Where(e => !inputExisting.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();

            return true;
        }
        public void LoadData(List<BookItem> asks, List<BookItem> bids)
        {
            #region Bids
            List<BookItem> addBids = new List<BookItem>();
            List<BookItem> delBids = new List<BookItem>();
            List<BookItem> updBids = new List<BookItem>();
            lock (_bidLock)
            {
                GetAddDeleteUpdate(_Bids, bids, out addBids, out updBids, out delBids);
                //_Bids = new ObservableCollectionEx<BookItem>(bids.OrderByDescending(x => x.Price).ToList(), _Bids.Comparison);
                foreach (var b in delBids)
                    _Bids.Remove(b);
                foreach (var b in updBids)
                {
                    var toUpdate = _Bids.Where(x => x.ProviderID == b.ProviderID && x.Symbol == b.Symbol && x.Price == b.Price).FirstOrDefault();
                    if (toUpdate != null)
                        toUpdate.Update(b);
                }
                foreach (var b in addBids)
                    _Bids.Add(b);
                _Bids.Sort();

                lock (_Cummulative_Bids)
                {
                    _Cummulative_Bids.Clear();
                    double cumSize = 0;
                    foreach (var o in _Bids.OrderByDescending(x => x.Price))
                    {
                        cumSize += o.Size;
                        _Cummulative_Bids.Add(new BookItem() { Price = o.Price, Size = cumSize, IsBid = true });
                    }
                }
            }
            //System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            //{
            //}));
            #endregion

            #region Asks
            lock (_askLock)
            {
                List<BookItem> addAsks = new List<BookItem>();
                List<BookItem> delAsks = new List<BookItem>();
                List<BookItem> updAsks = new List<BookItem>();
                GetAddDeleteUpdate(_Asks, asks, out addAsks, out updAsks, out delAsks);
                //_Asks = new ObservableCollectionEx<BookItem>(asks.OrderBy(x => x.Price).ToList(), _Asks.Comparison);
                foreach (var b in delAsks)
                    _Asks.Remove(b);
                foreach (var b in updAsks)
                {
                    var toUpdate = _Asks.Where(x => x.ProviderID == b.ProviderID && x.Symbol == b.Symbol && x.Price == b.Price).FirstOrDefault();
                    if (toUpdate != null)
                        toUpdate.Update(b);
                }
                foreach (var b in addAsks)
                    _Asks.Add(b);
                _Asks.Sort();



                lock (_Cummulative_Asks)
                {
                    _Cummulative_Asks.Clear();
                    double cumSize = 0;
                    foreach (var o in _Asks.OrderBy(x => x.Price))
                    {
                        cumSize += o.Size;
                        _Cummulative_Asks.Add(new BookItem() { Price = o.Price, Size = cumSize, IsBid = false });
                    }
                }
            }
            #endregion

        }


        public OrderBook() //emtpy constructor for JSON deserialization
        {
            var comparer = new Comparison<BookItem>((k1, k2) => k1.CompareTo(k2));
            _Asks = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer);
            _Cummulative_Asks = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer);

            var comparer2 = new Comparison<BookItem>((k1, k2) => k2.CompareTo(k1));
            _Bids = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer2);
            _Cummulative_Bids = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer2);
        }
        public OrderBook(string symbol, int decimalPlaces)
        {
            _Symbol = symbol;
            _DecimalPlaces = decimalPlaces;

            var comparer = new Comparison<BookItem>((k1, k2) => k1.CompareTo(k2));
            _Asks = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer);
            _Cummulative_Asks = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer);

            var comparer2 = new Comparison<BookItem>((k1, k2) => k2.CompareTo(k1));
            _Bids = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer2);
            _Cummulative_Bids = new ObservableCollectionEx<BookItem>(new List<BookItem>(), comparer2);

        }
        public ObservableCollectionEx<BookItem> Asks
        {
            get
            {
                lock (_askLock)
                    return _Asks;
            }
        }
        public ObservableCollectionEx<BookItem> Bids
        {
            get
            {
                lock (_bidLock)
                    return _Bids;                
            }
        }
        public string Symbol
        {
            get
            {
                return _Symbol;
            }

            set
            {
                if (_Symbol != value)
                {                    
                    _Symbol = value;
                    SetKey();
                    //RaisePropertyChanged("Symbol");
                }
            }
        }
        public int DecimalPlaces
        {
            get
            {
                return _DecimalPlaces;
            }
            set
            {
                if (_DecimalPlaces != value)
                {
                    _DecimalPlaces = value;
                    //RaisePropertyChanged("DecimalPlaces");
                }
            }
        }
        public double SymbolMultiplier
        {
            get
            {
                return _SymbolMultiplier;
            }
            set
            {
                if (_SymbolMultiplier != value)
                {
                    // get { return Math.Pow(10, _DecimalPlaces - 1); }
                    _SymbolMultiplier = value;
                    //RaisePropertyChanged("SymbolMultiplier");
                }
            }            
        }
        public int ProviderID {
            get => _ProviderID;
            set
            {
                if (_ProviderID != value)
                {
                    _ProviderID = value;
                    SetKey();
                    //RaisePropertyChanged("ProviderID");
                }
            }
        }
        public string ProviderName {
            get => _ProviderName;
            set
            {
                if (_ProviderName != value)
                {
                    _ProviderName = value;
                    //RaisePropertyChanged("ProviderName");
                }
            }
        }
        public eSESSIONSTATUS ProviderStatus { get => _providerStatus; set => _providerStatus = value; }
        public string KEY
        {
            get { return _KEY; }
        }


        public BookItem GetTOB(bool isBid)
        {
            if (isBid)
                lock(_bidLock)
                    return _Bids.FirstOrDefault();
            //return _Bids.OrderByDescending(x => x.Price).FirstOrDefault();
            else
                lock(_askLock)
                    return _Asks.FirstOrDefault();
            //return _Asks.OrderBy(x => x.Price).FirstOrDefault();
        }
        public double GetMaxOrderSize()
        {
            double _maxOrderSize = 0;

            lock (_bidLock)
            {
                if (_Bids != null)
                    _maxOrderSize = _Bids.DefaultIfEmpty(new BookItem()).Max(x => x.Size);
            }
            lock (_askLock)
            {
                if (_Asks != null)
                    _maxOrderSize = Math.Max(_maxOrderSize, _Asks.DefaultIfEmpty(new BookItem()).Max(x => x.Size));
            }
            return _maxOrderSize;
        }

        public ObservableCollectionEx<BookItem> BidCummulative
        {
            get { lock (_cumBidLock) { return _Cummulative_Bids; } }
        }
        public ObservableCollectionEx<BookItem> AskCummulative
        {
            get { lock (_cumAskLock) { return _Cummulative_Asks; } }
        }

    }
}
