using VisualHFT.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VisualHFT.Studies;
using VisualHFT.Helpers;

namespace VisualHFT.Model
{
    public partial class OrderBook : ICloneable, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed

        protected CachedCollection<BookItem> _Asks;
        protected CachedCollection<BookItem> _Bids;

        private CachedCollection<BookItem> _Cummulative_Bids;
        private CachedCollection<BookItem> _Cummulative_Asks;

        private object LOCK_OBJECT = new object();

        private string _Symbol;
        private int _DecimalPlaces;
        private double _SymbolMultiplier;
        private int _ProviderID;
        private string _ProviderName;
        private eSESSIONSTATUS _providerStatus;
        private double _MidPrice = 0;
        private double _Spread = 0;
        private BookItem _bidTOP = null;
        private BookItem _askTOP = null;
        public OrderBook() //emtpy constructor for JSON deserialization
        {
            _Cummulative_Asks = new CachedCollection<BookItem>();
            _Cummulative_Bids = new CachedCollection<BookItem>();
            _Bids = new CachedCollection<BookItem>();
            _Asks = new CachedCollection<BookItem>();
        }
        public OrderBook(string symbol, int decimalPlaces)
        {
            _Cummulative_Asks = new CachedCollection<BookItem>();
            _Cummulative_Bids = new CachedCollection<BookItem>();
            _Bids = new CachedCollection<BookItem>();
            _Asks = new CachedCollection<BookItem>();

            _Symbol = symbol;
            _DecimalPlaces = decimalPlaces;
        }
        ~OrderBook()
        {
            Dispose(false);
        }
        public void GetAddDeleteUpdate(ref ObservableCollection<BookItem> inputExisting, ReadOnlyCollection<BookItem> listToMatch)
        {
            ReadOnlyCollection<BookItem> inputNew = listToMatch;
            List<BookItem> outAdds;
            List<BookItem> outUpdates;
            List<BookItem> outRemoves;
            /*outRemoves = inputExisting.Where(e => !inputNew.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outUpdates = inputNew.Where(e => inputExisting.Any(i => i.Size != e.Size && i.Price == e.Price && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outAdds = inputNew.Where(e => !inputExisting.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            return true;*/

            var existingSet = inputExisting; // new HashSet<BookItem>(inputExisting);
            var newSet = inputNew; // new HashSet<BookItem>(inputNew);

            outRemoves = inputExisting.Where(e => !newSet.Contains(e)).ToList();
            outUpdates = inputNew.Where(e => existingSet.Contains(e) && e.Size != existingSet.First(i => i.Equals(e)).Size).ToList();
            outAdds = inputNew.Where(e => !existingSet.Contains(e)).ToList();

            foreach (var b in outRemoves)
                inputExisting.Remove(b);
            foreach (var b in outUpdates)
            {
                var itemToUpd = inputExisting.Where(x => x.Price == b.Price).FirstOrDefault();
                if (itemToUpd != null)
                {
                    itemToUpd.Size = b.Size;
                    itemToUpd.ActiveSize = b.ActiveSize;
                    itemToUpd.LocalTimeStamp = b.LocalTimeStamp;
                    itemToUpd.ServerTimeStamp = b.ServerTimeStamp;
                }
            }
            foreach (var b in outAdds)
                inputExisting.Add(b);
        }
        private void CalculateMetrics()
        {
            var lobMetrics = new OrderFlowAnalysis();
            lobMetrics.LoadData(_Asks.Where(x => x != null).ToList(), _Bids.Where(x => x != null).ToList());
            this.ImbalanceValue = lobMetrics.Calculate_OrderImbalance();
        }
        public bool LoadData()
        {
            return LoadData(this.Asks, this.Bids);
        }
        public bool LoadData(IEnumerable<BookItem> asks, IEnumerable<BookItem> bids)
        {
            bool ret = true;
            lock (LOCK_OBJECT)
            {
                #region Bids
                if (bids != null)
                    _Bids.Update(bids.Where(x => x.Price.HasValue).OrderByDescending(x => x.Price));
                _Cummulative_Bids.Clear();
                double cumSize = 0;
                foreach (var o in _Bids.Where(x => x.Price.HasValue && x.Size.HasValue).OrderByDescending(x => x.Price))
                {
                    cumSize += o.Size.Value;
                    _Cummulative_Bids.Add(new BookItem() { Price = o.Price, Size = cumSize, IsBid = true });
                }
                #endregion

                #region Asks
                if (asks != null)
                    _Asks.Update(asks.Where(x => x.Price.HasValue).OrderBy(x => x.Price));
                _Cummulative_Asks.Clear();
                cumSize = 0;
                foreach (var o in _Asks.Where(x => x.Price.HasValue && x.Size.HasValue).OrderBy(x => x.Price))
                {
                    cumSize += o.Size.Value;
                    _Cummulative_Asks.Add(new BookItem() { Price = o.Price, Size = cumSize, IsBid = false });
                }
                #endregion

                _bidTOP = _Bids.FirstOrDefault();
                _askTOP = _Asks.FirstOrDefault();
                if (_bidTOP != null && _bidTOP.Price.HasValue && _askTOP != null && _askTOP.Price.HasValue)
                {
                    _MidPrice = (_bidTOP.Price.Value + _askTOP.Price.Value) / 2;
                    _Spread = _askTOP.Price.Value - _bidTOP.Price.Value;
                }

                CalculateMetrics();
            }
            return ret;
        }
        public ReadOnlyCollection<BookItem> Asks
        {
            get => _Asks.AsReadOnly();
            set => _Asks.Update(value); //do not remove setter: it is used to auto parse json
        }
        public ReadOnlyCollection<BookItem> Bids 
        { 
            get => _Bids.AsReadOnly();
            set => _Bids.Update(value); //do not remove setter: it is used to auto parse json
        }
        public ReadOnlyCollection<BookItem> BidCummulative => _Cummulative_Bids.AsReadOnly();
        public ReadOnlyCollection<BookItem> AskCummulative => _Cummulative_Asks.AsReadOnly();

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


        public BookItem GetTOB(bool isBid)
        {
            lock (LOCK_OBJECT)
            {
                if (isBid)
                    return _bidTOP;
                else
                    return _askTOP;
            }
        }
        public double GetMaxOrderSize()
        {
            double _maxOrderSize = 0;

            lock (LOCK_OBJECT)
            {
                if (_Bids != null)
                    _maxOrderSize = _Bids.Where(x => x.Size.HasValue).DefaultIfEmpty(new BookItem()).Max(x => x.Size.Value);
                if (_Asks != null)
                    _maxOrderSize = Math.Max(_maxOrderSize, _Asks.Where(x => x.Size.HasValue).DefaultIfEmpty(new BookItem()).Max(x => x.Size.Value));
            }
            return _maxOrderSize;
        }
        public Tuple<double, double> GetMinMaxSizes()
        {
            List<BookItem> allOrders = new List<BookItem>();

            lock (LOCK_OBJECT)
            {
                if (_Bids != null)
                    allOrders.AddRange(_Bids.Where(x => x.Size.HasValue).ToList());
                if (_Asks != null)  
                    allOrders.AddRange(_Asks.Where(x => x.Size.HasValue).ToList());
            }
            //AVOID OUTLIERS IN SIZES (when data is invalid)
            double firstQuantile = allOrders.Select(x => x.Size.Value).Quantile(0.25);
            double thirdQuantile = allOrders.Select(x => x.Size.Value).Quantile(0.75);
            double iqr = thirdQuantile - firstQuantile;
            double lowerBand = firstQuantile - 1.5 * iqr;
            double upperBound = thirdQuantile + 1.5 * iqr;

            double minOrderSize = allOrders.Where(x => x.Size >= lowerBand).Min(x => x.Size.Value);
            double maxOrderSize = allOrders.Where(x => x.Size <= upperBound).Max(x => x.Size.Value);

            return Tuple.Create(minOrderSize, maxOrderSize);
        }

        public object Clone()
        {
            var clone = new OrderBook
            {
                DecimalPlaces = DecimalPlaces,
                ProviderID=ProviderID,
                ProviderName=ProviderName,
                Symbol = Symbol,
                SymbolMultiplier = SymbolMultiplier,
            };
            clone.LoadData(Asks, Bids);
            return clone;
        }

        public double ImbalanceValue { get; set; }
        public double MidPrice { get => _MidPrice;  }
        public double Spread { get => _Spread; }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _Cummulative_Asks?.Clear();
                    _Cummulative_Bids?.Clear();
                    _Bids?.Clear();
                    _Asks?.Clear();


                    _Cummulative_Asks = null;
                    _Cummulative_Bids= null;
                    _Bids = null;
                    _Asks = null;

                    _bidTOP = null;
                    _askTOP = null;
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
