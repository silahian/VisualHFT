using VisualHFT.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace VisualHFT.Model
{
    public class OrderBook
    {
        protected List<BookItem> _Bids;
        protected List<BookItem> _Asks;

        private List<BookItem> _Cummulative_Bids;
        private List<BookItem> _Cummulative_Asks;

        static object LOCK_OBJECT = new object();

        private string _KEY;
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

        private void SetKey()
        {
            if (this.ProviderID >= -1 && this.Symbol != "")
            {
                _KEY = this.ProviderID.ToString() + "_" + this.Symbol;
            }
        }
        private bool GetAddDeleteUpdate(ObservableCollectionEx<BookItem> inputExisting, List<BookItem> inputNew, out List<BookItem> outAdds, out List<BookItem> outUpdates, out List<BookItem> outRemoves)
        {
            /*outRemoves = inputExisting.Where(e => !inputNew.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outUpdates = inputNew.Where(e => inputExisting.Any(i => i.Size != e.Size && i.Price == e.Price && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();
            outAdds = inputNew.Where(e => !inputExisting.Any(i => i.Price == e.Price && i.Size == e.Size && i.ProviderID == e.ProviderID && i.Symbol == e.Symbol)).ToList();

            return true;*/
            var existingSet = new HashSet<BookItem>(inputExisting);
            var newSet = new HashSet<BookItem>(inputNew);

            outRemoves = inputExisting.Where(e => !newSet.Contains(e)).ToList();
            outUpdates = inputNew.Where(e => existingSet.Contains(e) && e.Size != existingSet.First(i => i.Equals(e)).Size).ToList();
            outAdds = inputNew.Where(e => !existingSet.Contains(e)).ToList();

            return true;
        }
        private void MakeEqualLenght()
        {
            var largestLength = Math.Max(_Asks.Count, _Bids.Count);
            while (_Asks.Count != largestLength)
                _Asks.Add(new BookItem());
            while (_Bids.Count != largestLength)
                _Bids.Add(new BookItem());
        }
        private void CalculateMetrics()
        {
            var lobMetrics = new OrderFlowAnalysis();
            lobMetrics.LoadData(_Asks.Where(x => x != null).ToList(), _Bids.Where(x => x != null).ToList());
            this.ImbalanceValue = lobMetrics.Calculate_OrderImbalance(); 
        }
        public bool LoadData(List<BookItem> asks, List<BookItem> bids)
        {
            bool ret = true;
            lock (LOCK_OBJECT)
            {
                #region Bids
                if (bids != null)
                    _Bids = new List<BookItem>(bids.Where(x => x.Price.HasValue).OrderByDescending(x => x.Price).ToList());
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
                    _Asks = new List<BookItem>(asks.Where(x => x.Price.HasValue).OrderBy(x => x.Price).ToList());
                _Cummulative_Asks.Clear();
                cumSize = 0;
                foreach (var o in _Asks.Where(x => x.Price.HasValue && x.Size.HasValue).OrderBy(x => x.Price))
                {
                    cumSize += o.Size.Value;
                    _Cummulative_Asks.Add(new BookItem() { Price = o.Price, Size = cumSize, IsBid = false });
                }
                #endregion
                
                _bidTOP  = _Bids.FirstOrDefault();
                _askTOP = _Asks.FirstOrDefault();
                if (_bidTOP != null && _bidTOP.Price.HasValue && _askTOP != null && _askTOP.Price.HasValue)
                {
                    _MidPrice = (_bidTOP.Price.Value + _askTOP.Price.Value) / 2;
                    _Spread = _askTOP.Price.Value - _bidTOP.Price.Value;
                }

                MakeEqualLenght(); // to avoid grid flickering
                CalculateMetrics();
            }
            return ret;
        }


        public OrderBook() //emtpy constructor for JSON deserialization
        {
            _Cummulative_Bids = new List<BookItem>();
            _Cummulative_Asks = new List<BookItem>();
        }
        public OrderBook(string symbol, int decimalPlaces)
        {
            _Symbol = symbol;
            _DecimalPlaces = decimalPlaces;
        }
        public List<BookItem> Asks
        {
            get
            {
                lock (LOCK_OBJECT)
                    return _Asks;
            }
            set { lock (LOCK_OBJECT) _Asks = value; }
        }
        public List<BookItem> Bids
        {
            get
            {
                lock (LOCK_OBJECT)
                    return _Bids;
            }
            set { lock (LOCK_OBJECT) _Bids = value; }
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


        public List<BookItem> BidCummulative
        {
            get { lock (LOCK_OBJECT) { return new List<BookItem>(_Cummulative_Bids); } }
        }
        public List<BookItem> AskCummulative
        {
            get { lock (LOCK_OBJECT) { return new List<BookItem>(_Cummulative_Asks); } }
        }
        public double ImbalanceValue { get; set; }
        public double MidPrice { get => _MidPrice;  }
        public double Spread { get => _Spread; }

    }
}
