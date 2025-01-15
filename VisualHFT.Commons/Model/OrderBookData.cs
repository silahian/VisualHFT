using VisualHFT.Commons.Model;
using VisualHFT.Commons.Pools;
using VisualHFT.Helpers;
using VisualHFT.Enums;

namespace VisualHFT.Model
{
    public class OrderBookData : IResettable, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private CachedCollection<BookItem> _Asks;
        private CachedCollection<BookItem> _Bids;
        public CachedCollection<BookItem> Asks
        {
            get
            {
                return _Asks;
            }
        }
        public CachedCollection<BookItem> Bids
        {
            get
            {
                return _Bids;
            }
        }
        public string Symbol { get; set; }
        public int PriceDecimalPlaces { get; set; }
        public int SizeDecimalPlaces { get; set; }
        public double SymbolMultiplier => Math.Pow(10, PriceDecimalPlaces);
        public int ProviderID { get; set; }
        public string ProviderName { get; set; }
        public eSESSIONSTATUS ProviderStatus { get; set; }
        public int MaxDepth { get; set; }
        public double MaximumCummulativeSize { get; set; }
        public double ImbalanceValue { get; set; }
        public object Lock { get; } = new object();
        public BookItem GetTOB(bool isBid)
        {
            if (isBid)
                return Bids?.FirstOrDefault();
            else
                return Asks?.FirstOrDefault();
        }
        public double MidPrice
        {
            get
            {
                var _bidTOP = GetTOB(true);
                var _askTOP = GetTOB(false);
                if (_bidTOP != null && _bidTOP.Price.HasValue && _askTOP != null && _askTOP.Price.HasValue)
                {
                    return (_bidTOP.Price.Value + _askTOP.Price.Value) / 2.0;
                }
                return 0;
            }
        }
        public double Spread
        {
            get
            {
                var _bidTOP = GetTOB(true);
                var _askTOP = GetTOB(false);
                if (_bidTOP != null && _bidTOP.Price.HasValue && _askTOP != null && _askTOP.Price.HasValue)
                {
                    return _askTOP.Price.Value - _bidTOP.Price.Value;
                }
                return 0;
            }
        }
        public bool FilterBidAskByMaxDepth { get; set; }

        public OrderBookData()
        {
            _Bids = new CachedCollection<BookItem>((x, y) => y.Price.GetValueOrDefault().CompareTo(x.Price.GetValueOrDefault()));
            _Asks = new CachedCollection<BookItem>((x, y) => x.Price.GetValueOrDefault().CompareTo(y.Price.GetValueOrDefault()));
        }

        public OrderBookData(string symbol, int priceDecimalPlaces, int maxDepth)
        {
            _Bids = new CachedCollection<BookItem>((x, y) => y.Price.GetValueOrDefault().CompareTo(x.Price.GetValueOrDefault()), maxDepth);
            _Asks = new CachedCollection<BookItem>((x, y) => x.Price.GetValueOrDefault().CompareTo(y.Price.GetValueOrDefault()), maxDepth);

            MaxDepth = maxDepth;
            Symbol = symbol;
            PriceDecimalPlaces = priceDecimalPlaces;
        }
        public void CalculateAccummulated()
        {
            double cumSize = 0;
            foreach (var o in Bids)
            {
                cumSize += o.Size.Value;
                o.CummulativeSize = cumSize;
            }

            MaximumCummulativeSize = cumSize;

            cumSize = 0;
            foreach (var o in Asks)
            {
                cumSize += o.Size.Value;
                o.CummulativeSize = cumSize;
            }
            MaximumCummulativeSize = Math.Max(MaximumCummulativeSize, cumSize);
        }
        public Tuple<double, double> GetMinMaxSizes()
        {
            List<BookItem> allOrders = new List<BookItem>();
            double minVal = 0;
            double maxVal = 0;
            if (Asks == null || Bids == null
                             || Asks.Count() == 0
                             || Bids.Count() == 0)
                return new Tuple<double, double>(0, 0);

            foreach (var o in Bids)
            {
                if (o.Size.HasValue)
                {
                    minVal = Math.Min(minVal, o.Size.Value);
                    maxVal = Math.Max(maxVal, o.Size.Value);
                }
            }
            foreach (var o in Asks)
            {
                if (o.Size.HasValue)
                {
                    minVal = Math.Min(minVal, o.Size.Value);
                    maxVal = Math.Max(maxVal, o.Size.Value);
                }
            }

            return Tuple.Create(minVal, maxVal);
        }


        public void ShallowCopyFrom(OrderBook e, CustomObjectPool<BookItem> pool)
        {
            if (e == null)
                return;
            Symbol = e.Symbol;
            PriceDecimalPlaces = e.PriceDecimalPlaces;
            SizeDecimalPlaces = e.SizeDecimalPlaces;
            ProviderID = e.ProviderID;
            ProviderName = e.ProviderName;
            ProviderStatus = e.ProviderStatus;
            MaxDepth = e.MaxDepth;
            ImbalanceValue = e.ImbalanceValue;

            _Asks.ShallowCopyFrom(e.Asks, pool);
            _Bids.ShallowCopyFrom(e.Bids, pool);
        }

        /// <summary>
        /// ShallowUpdateFrom
        /// Will update the existing data.
        /// This is very useful when keeping a Collection locally and want to avoid swapping and allocating
        /// </summary>
        /// <param name="sourceList">The source list.</param>
        public void ShallowUpdateFrom(OrderBook e)
        {
            if (e == null)
                return;
            Symbol = e.Symbol;
            PriceDecimalPlaces = e.PriceDecimalPlaces;
            SizeDecimalPlaces = e.SizeDecimalPlaces;
            ProviderID = e.ProviderID;
            ProviderName = e.ProviderName;
            ProviderStatus = e.ProviderStatus;
            MaxDepth = e.MaxDepth;
            ImbalanceValue = e.ImbalanceValue;

            _Asks.ShallowUpdateFrom(e.Asks);
            _Bids.ShallowUpdateFrom(e.Bids);
        }

        public void Reset()
        {
            _Bids?.Clear();
            _Asks?.Clear();
            Symbol = "";
            PriceDecimalPlaces = 0;
            SizeDecimalPlaces = 0;
            ProviderID = 0;
            ProviderName = "";
            MaxDepth = 0;
        }
        public void Clear()
        {
            Bids?.Clear();
            Asks?.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _Bids?.Clear();
                    _Asks?.Clear();
                    _Bids = null;
                    _Asks = null;
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