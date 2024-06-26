using System.Collections;
using System.Collections.ObjectModel;
using VisualHFT.Enums;


/*
 * The AggregatedCollection<T> class is designed to maintain a running window list of items with a specified maximum capacity.
 * It ensures that as new items are added, the oldest items are removed once the capacity is reached, effectively implementing a Last-In-First-Out (LIFO) collection.
 * Additionally, it supports aggregation of items based on a specified aggregation level (e.g., 10 milliseconds, 20 milliseconds).
 * Each item in the collection represents a bucket corresponding to the chosen aggregation level, aggregating data within the same bucket based on the provided dateSelector and aggregator functions.
 *
 */
namespace VisualHFT.Helpers
{
    public class AggregatedCollection<T> : IDisposable, IEnumerable<T> where T : class, new()
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private TimeSpan _aggregationSpan;
        private TimeSpan _dynamicAggregationSpan;
        private AggregationLevel _level;
        //private readonly CachedCollection<T> _aggregatedData;
        private readonly List<T> _aggregatedData;

        private readonly Func<T, DateTime> _dateSelector;
        private readonly Action<T, T, int> _aggregator;

        private readonly object _lockObject = new object();
        private int _maxPoints = 0; // Maximum number of points
        private DateTime lastItemDate = DateTime.MinValue;
        private int _ItemsUpdatedCount = 0;

        //AUTOMATED Aggregation
        private const int WINDOW_SIZE = 10; // Number of items to consider for frequency calculation

        public event EventHandler<T> OnRemoving;
        public event EventHandler<int> OnRemoved;
        public event EventHandler<T> OnAdded;

        public AggregatedCollection(IEnumerable<T> items, AggregationLevel level, int maxItems, Func<T, DateTime> dateSelector, Action<T, T, int> aggregator)
        {
            if (items != null)
                _aggregatedData = new List<T>(items);
            else
                _aggregatedData = new List<T>(maxItems);

            _maxPoints = maxItems;
            _level = level;
            _aggregationSpan = level.ToTimeSpan();
            _dynamicAggregationSpan = _aggregationSpan; // Initialize with the same value
            _dateSelector = dateSelector;
            _aggregator = aggregator;
        }
        public AggregatedCollection(AggregationLevel level, int maxItems, Func<T, DateTime> dateSelector, Action<T, T, int> aggregator)
            : this(null, level, maxItems, dateSelector, aggregator)
        {
        }
        public AggregatedCollection(AggregationLevel level, int maxItems, Func<T, DateTime> dateSelector, Action<T, T> aggregator)
            : this(null, level, maxItems, dateSelector, (a, b, _) => aggregator(a, b))
        {
        }
        ~AggregatedCollection()
        {
            Dispose(false);
        }



        public bool Add(T item)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AggregatedCollection<T>));
            }

            bool retValue = false;
            int? removedItemIndexToSendEvent = null;
            T addedItemToSendEvent = null;
            T removingItemToSendEvent = null;

            lock (_lockObject)
            {
                if (_aggregationSpan == TimeSpan.Zero /*&& _level != AggregationLevel.Automatic*/)
                {
                    _aggregatedData.Add(item);
                    addedItemToSendEvent = item;

                    if (_aggregatedData.Count() > _maxPoints)
                    {
                        // Remove the item from the collection
                        T itemToRemove = _aggregatedData[0];

                        removingItemToSendEvent = itemToRemove;

                        _aggregatedData.Remove(itemToRemove);

                        // Trigger any remove events or perform additional logic as required
                        removedItemIndexToSendEvent = 0;
                    }
                    retValue = true;
                }
                else
                {
                    bool _readyToAdd = true;
                    T lastItem = _aggregatedData.LastOrDefault();
                    if (lastItem != null)
                    {
                        //diff in timestamp between the incoming item and lastItem
                        /*if (_level == AggregationLevel.Automatic)
                        {
                            _dynamicAggregationSpan = CalculateAutomaticAggregationSpan();
                            _readyToAdd = Math.Abs(_dateSelector(item).Ticks - _dateSelector(lastItem).Ticks) >= _dynamicAggregationSpan.Ticks;
                        }
                        else*/
                        {
                            _readyToAdd = Math.Abs(_dateSelector(item).Ticks - _dateSelector(lastItem).Ticks) >= _level.ToTimeSpan().Ticks;
                        }
                    }

                    // Check the last item in the list
                    if (!_readyToAdd)
                    {
                        _ItemsUpdatedCount++;
                        _aggregator(lastItem, item, _ItemsUpdatedCount);
                        retValue = false;
                    }
                    else
                    {
                        _ItemsUpdatedCount = 0; //reset on add new
                        _aggregatedData.Add(item);
                        addedItemToSendEvent = item;

                        if (_aggregatedData.Count() > _maxPoints)
                        {
                            T itemToRemove = _aggregatedData[0];

                            // Remove the item from the collection
                            removingItemToSendEvent = itemToRemove;

                            _aggregatedData.Remove(itemToRemove);

                            // Trigger any remove events or perform additional logic as required
                            removedItemIndexToSendEvent = 0;
                        }

                        retValue = true;
                    }

                }
            }


            //SEND ALL EVENTS OUT FROM THE LOCK
            if (removedItemIndexToSendEvent.HasValue)
                OnRemoved?.Invoke(this, removedItemIndexToSendEvent.Value);
            if (removingItemToSendEvent != null)
                OnRemoving?.Invoke(this, removingItemToSendEvent);
            if (addedItemToSendEvent != null)
                OnAdded?.Invoke(this, addedItemToSendEvent);

            return retValue;
        }

        public void Clear()
        {
            lock (_lockObject)
                _aggregatedData.Clear();
            OnRemoved?.Invoke(this, -1); //-1 indicates that we are clearing the list
        }
        public int Count()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Count();
            }
        }
        public bool Any()
        {
            lock (_lockObject)
            {
                return _aggregatedData.Count > 0;
            }
        }
        public IEnumerable<T> ToList()
        {
            lock (_lockObject)
            {
                return _aggregatedData.ToList();
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_lockObject)
                {
                    return _aggregatedData[index];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lockObject)
            {
                return _aggregatedData.ToList().GetEnumerator(); // Avoid direct enumeration on the locked collection
            }
        }


        public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            lock (_lockObject)
            {
                return _aggregatedData.Select(selector);
            }
        }
        public IEnumerable<TResult> SelectMany<TResult>(Func<T, IEnumerable<TResult>> selector)
        {
            lock (_lockObject)
            {
                return _aggregatedData.SelectMany(selector);
            }
        }
        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_lockObject)
            {
                return _aggregatedData.AsReadOnly();
            }
        }
        public T LastOrDefault()
        {
            lock (_lockObject)
                return _aggregatedData.LastOrDefault();

        }
        public T FirstOrDefault()
        {
            lock (_lockObject)
                return _aggregatedData.FirstOrDefault();
        }
        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            lock (_lockObject)
                return _aggregatedData.Where(predicate);
        }
        public decimal Min(Func<T, decimal> selector)
        {
            lock (_lockObject)
                return _aggregatedData.DefaultIfEmpty(new T()).Min(selector);
        }
        public double Min(Func<T, double> selector)
        {
            lock (_lockObject)
                return _aggregatedData.DefaultIfEmpty(new T()).Min(selector);
        }
        public decimal Max(Func<T, decimal> selector)
        {
            lock (_lockObject)
                return _aggregatedData.DefaultIfEmpty(new T()).Max(selector);
        }
        public double Max(Func<T, double> selector)
        {
            lock (_lockObject)
                return _aggregatedData.DefaultIfEmpty(new T()).Max(selector);
        }
        public TimeSpan DynamicAggregationSpan => _dynamicAggregationSpan;


        /// <summary>
        /// Calculates the automatic aggregation span.
        /// The CalculateAutomaticAggregationSpan method dynamically adjusts the aggregation span of the AggregatedCollection<T> based on the rate of incoming data.
        /// It calculates the average time interval between the timestamps of the last few items (using a sliding window approach)
        ///  to determine an appropriate aggregation span. This allows the collection to adapt to varying data frequencies,
        ///  ensuring that items are aggregated appropriately according to the observed data rate.
        /// The method returns a TimeSpan that represents the calculated aggregation level, which can range from 1 millisecond to 1 day,
        ///  based on predefined thresholds.
        /// </summary>
        /// <param name="currentItemDate">The current item date.</param>
        /// <returns>A TimeSpan.</returns>
        internal TimeSpan CalculateAutomaticAggregationSpan()
        {
            if (_aggregatedData.Count <= 1)
                return AggregationLevel.Ms1.ToTimeSpan(); //minimum available
            int _window = Math.Min(WINDOW_SIZE, _aggregatedData.Count);
            var lastWindowList = _aggregatedData.Select(x => _dateSelector(x)).TakeLast(_window);

            var avgTimeDiffs = lastWindowList
                .Zip(lastWindowList.Skip(1), (first, second) => (second - first)).Average(x => x.Milliseconds);

            var averageElapsed = TimeSpan.FromMilliseconds(avgTimeDiffs);

            if (averageElapsed <= TimeSpan.FromMilliseconds(1))
                return AggregationLevel.Ms1.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromMilliseconds(10))
                return AggregationLevel.Ms10.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromMilliseconds(100))
                return AggregationLevel.Ms100.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromMilliseconds(500))
                return AggregationLevel.Ms500.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromSeconds(1))
                return AggregationLevel.S1.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromSeconds(3))
                return AggregationLevel.S3.ToTimeSpan();
            else if (averageElapsed <= TimeSpan.FromSeconds(5))
                return AggregationLevel.S5.ToTimeSpan();
            else
                return AggregationLevel.D1.ToTimeSpan();
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _aggregatedData?.Clear();
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
