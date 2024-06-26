using VisualHFT.Commons.Model;
using VisualHFT.Commons.Pools;
using VisualHFT.Helpers;
using VisualHFT.Studies;
using VisualHFT.Enums;
using VisualHFT.Model;

namespace VisualHFT.Model
{
    public partial class OrderBook : ICloneable, IResettable, IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private OrderFlowAnalysis lobMetrics = new OrderFlowAnalysis();

        protected OrderBookData _data;
        protected static readonly log4net.ILog log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected CustomObjectPool<BookItem> _poolBookItems
            = new CustomObjectPool<BookItem>(2000);


        public OrderBook()
        {
            _data = new OrderBookData();
        }

        public OrderBook(string symbol, int priceDecimalPlaces, int maxDepth)
        {
            _data = new OrderBookData(symbol, priceDecimalPlaces, maxDepth);
        }

        ~OrderBook()
        {
            Dispose(false);
        }

        public string Symbol
        {
            get => _data.Symbol;
            set => _data.Symbol = value;
        }

        public int MaxDepth
        {
            get => _data.MaxDepth;
            set => _data.MaxDepth = value;
        }

        public int PriceDecimalPlaces
        {
            get => _data.PriceDecimalPlaces;
            set => _data.PriceDecimalPlaces = value;
        }

        public int SizeDecimalPlaces
        {
            get => _data.SizeDecimalPlaces;
            set => _data.SizeDecimalPlaces = value;
        }

        public double SymbolMultiplier => _data.SymbolMultiplier;

        public int ProviderID
        {
            get => _data.ProviderID;
            set => _data.ProviderID = value;
        }

        public string ProviderName
        {
            get => _data.ProviderName;
            set => _data.ProviderName = value;
        }

        public eSESSIONSTATUS ProviderStatus
        {
            get => _data.ProviderStatus;
            set => _data.ProviderStatus = value;
        }

        public double MaximumCummulativeSize
        {
            get => _data.MaximumCummulativeSize;
            set => _data.MaximumCummulativeSize = value;
        }

        public CachedCollection<BookItem> Asks
        {
            get
            {
                lock (_data.Lock)
                {
                    if (_data.Asks == null)
                        return null;
                    if (FilterBidAskByMaxDepth)
                        return _data.Asks.Take(MaxDepth);
                    else
                        return _data.Asks;
                }
            }
            set => _data.Asks.Update(value); //do not remove setter: it is used to auto parse json
        }

        public CachedCollection<BookItem> Bids
        {
            get
            {
                lock (_data.Lock)
                {
                    if (_data.Bids == null)
                        return null;
                    if (FilterBidAskByMaxDepth)
                        return _data.Bids.Take(MaxDepth);
                    else
                        return _data.Bids;
                }
            }
            set => _data.Bids.Update(value); //do not remove setter: it is used to auto parse json
        }

        public BookItem GetTOB(bool isBid)
        {
            lock (_data.Lock)
            {
                return _data.GetTOB(isBid);
            }
        }

        public double MidPrice
        {
            get
            {
                return _data.MidPrice;
            }
        }
        public double Spread
        {
            get
            {
                return _data.Spread;
            }
        }
        public bool FilterBidAskByMaxDepth
        {
            get
            {
                return _data.FilterBidAskByMaxDepth;
            }
            set
            {
                _data.FilterBidAskByMaxDepth = value;
            }
        }
        public void GetAddDeleteUpdate(ref CachedCollection<BookItem> inputExisting, bool matchAgainsBids)
        {
            if (inputExisting == null)
                return;
            lock (_data.Lock)
            {
                IEnumerable<BookItem> listToMatch = (matchAgainsBids ? _data.Bids : _data.Asks);
                if (listToMatch.Count() == 0)
                    return;

                if (inputExisting.Count() == 0)
                {
                    foreach (var item in listToMatch)
                    {
                        inputExisting.Add(item);
                    }

                    return;
                }

                IEnumerable<BookItem> inputNew = listToMatch;
                List<BookItem> outAdds;
                List<BookItem> outUpdates;
                List<BookItem> outRemoves;

                var existingSet = inputExisting;
                var newSet = inputNew;

                outRemoves = inputExisting.Where(e => !newSet.Contains(e)).ToList();
                outUpdates = inputNew.Where(e =>
                    existingSet.Contains(e) && e.Size != existingSet.FirstOrDefault(i => i.Equals(e)).Size).ToList();
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
                        itemToUpd.CummulativeSize = b.CummulativeSize;
                        itemToUpd.LocalTimeStamp = b.LocalTimeStamp;
                        itemToUpd.ServerTimeStamp = b.ServerTimeStamp;
                    }
                }

                foreach (var b in outAdds)
                    inputExisting.Add(b);
            }
        }

        public void CalculateMetrics()
        {
            lock (_data.Lock)
            {
                lobMetrics.LoadData(_data.Asks, _data.Bids, MaxDepth);
            }
            _data.ImbalanceValue = lobMetrics.Calculate_OrderImbalance();
        }
        public bool LoadData(IEnumerable<BookItem> asks, IEnumerable<BookItem> bids)
        {
            bool ret = true;
            lock (_data.Lock)
            {
                #region Bids
                if (bids != null)
                {
                    _data.Bids.Update(bids
                        .Where(x => x != null && x.Price.HasValue && x.Size.HasValue)
                        .OrderByDescending(x => x.Price.Value)
                    );
                }
                #endregion
                #region Asks
                if (asks != null)
                {
                    _data.Asks.Update(asks
                        .Where(x => x != null && x.Price.HasValue && x.Size.HasValue)
                        .OrderBy(x => x.Price.Value)
                    );
                }
                #endregion
                _data.CalculateAccummulated();
            }
            CalculateMetrics();

            return ret;
        }

        public double GetMaxOrderSize()
        {
            double _maxOrderSize = 0;

            lock (_data.Lock)
            {
                if (_data.Bids != null)
                    _maxOrderSize = _data.Bids.Where(x => x.Size.HasValue).DefaultIfEmpty(new BookItem()).Max(x => x.Size.Value);
                if (_data.Asks != null)
                    _maxOrderSize = Math.Max(_maxOrderSize, _data.Asks.Where(x => x.Size.HasValue).DefaultIfEmpty(new BookItem()).Max(x => x.Size.Value));
            }
            return _maxOrderSize;
        }

        public Tuple<double, double> GetMinMaxSizes()
        {
            lock (_data.Lock)
            {
                return _data.GetMinMaxSizes();
            }
        }

        public virtual object Clone()
        {
            var clone = new OrderBook(_data.Symbol, _data.PriceDecimalPlaces, _data.MaxDepth);
            clone.ProviderID = _data.ProviderID;
            clone.ProviderName = _data.ProviderName;
            clone.SizeDecimalPlaces = _data.SizeDecimalPlaces;
            clone._data.ImbalanceValue = _data.ImbalanceValue;
            clone.ProviderStatus = _data.ProviderStatus;
            clone.MaxDepth = _data.MaxDepth;
            clone.LoadData(Asks, Bids);
            return clone;
        }

        public void PrintLOB(bool isBid)
        {
            lock (_data.Lock)
            {
                int _level = 0;
                foreach (var item in isBid ? _data.Bids : _data.Asks)
                {
                    Console.WriteLine($"{_level} - {item.FormattedPrice} [{item.Size}]");
                    _level++;
                }
            }
        }

        public double ImbalanceValue
        {
            get => _data.ImbalanceValue;
            set => _data.ImbalanceValue = value;
        }

        public void ShallowCopyFrom(OrderBook e, CustomObjectPool<BookItem> pool)
        {
            if (e == null)
                return;
            _data.ShallowCopyFrom(e, pool);
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
            _data.ShallowUpdateFrom(e);
        }

        public void Clear()
        {
            lock (_data.Lock)
            {
                while (_data.Asks.Count() > 0)
                    DeleteLevel(new DeltaBookItem() { IsBid = false, Price = Asks[0].Price });
                while (_data.Bids.Count() > 0)
                    DeleteLevel(new DeltaBookItem() { IsBid = true, Price = Bids[0].Price });

                _data.Clear();
            }
        }
        public void Reset()
        {
            lock (_data.Lock)
            {
                while (_data.Asks.Count() > 0)
                    DeleteLevel(new DeltaBookItem() { IsBid = false, Price = Asks[0].Price });
                while (_data.Bids.Count() > 0)
                    DeleteLevel(new DeltaBookItem() { IsBid = true, Price = Bids[0].Price });

                _data?.Reset();
            }

        }



        public virtual void AddOrUpdateLevel(DeltaBookItem item)
        {
            if (!item.IsBid.HasValue)
                return;
            eMDUpdateAction eAction = eMDUpdateAction.None;

            lock (_data.Lock)
            {
                var _list = (item.IsBid.HasValue && item.IsBid.Value ? _data.Bids : _data.Asks);
                var itemFound = _list.FirstOrDefault(x => x.Price == item.Price);
                if (itemFound == null)
                    eAction = eMDUpdateAction.New;
                else
                    eAction = eMDUpdateAction.Change;
            }

            if (eAction == eMDUpdateAction.Change)
                UpdateLevel(item);
            else
                AddLevel(item);

        }
        public virtual void AddLevel(DeltaBookItem item)
        {
            if (!item.IsBid.HasValue)
                return;

            lock (_data.Lock)
            {
                var list = item.IsBid.Value ? _data.Bids : _data.Asks;
                if (string.IsNullOrEmpty(_poolBookItems.ProviderName))
                    _poolBookItems.ProviderName = _data.ProviderName;
                var _level = _poolBookItems.Get();
                _level.EntryID = item.EntryID;
                _level.Price = item.Price;
                _level.IsBid = item.IsBid.Value;
                _level.LocalTimeStamp = item.LocalTimeStamp;
                _level.ProviderID = _data.ProviderID;
                _level.ServerTimeStamp = item.ServerTimeStamp;
                _level.Size = item.Size;
                _level.Symbol = _data.Symbol;
                _level.PriceDecimalPlaces = this.PriceDecimalPlaces;
                _level.SizeDecimalPlaces = this.SizeDecimalPlaces;
                list.Add(_level);
            }
        }
        public virtual void UpdateLevel(DeltaBookItem item)
        {
            lock (_data.Lock)
            {
                (item.IsBid.HasValue && item.IsBid.Value ? _data.Bids : _data.Asks).Update(x => x.Price == item.Price,
                    existingItem =>
                    {
                        existingItem.Price = item.Price;
                        existingItem.Size = item.Size;
                        existingItem.LocalTimeStamp = item.LocalTimeStamp;
                        existingItem.ServerTimeStamp = item.ServerTimeStamp;
                    });
            }

        }
        public virtual void DeleteLevel(DeltaBookItem item)
        {
            if (!item.Price.HasValue || item.Price.Value == 0)
                throw new Exception("DeltaBookItem cannot be deleted since has no price.");
            lock (_data.Lock)
            {
                var _itemToDelete =
                    (item.IsBid.HasValue && item.IsBid.Value ? _data.Bids : _data.Asks)
                    .FirstOrDefault(x => x.Price == item.Price);
                if (_itemToDelete != null)
                {
                    (item.IsBid.HasValue && item.IsBid.Value ? _data.Bids : _data.Asks).Remove(_itemToDelete);
                    _poolBookItems.Return(_itemToDelete);
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _data?.Dispose();
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

