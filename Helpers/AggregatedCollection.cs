using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisualHFT.Model;

namespace VisualHFT.Helpers
{
    public class AggregatedCollection<T> : IDisposable
    {
        private bool _disposed = false; // to track whether the object has been disposed
        private TimeSpan _aggregationSpan;
        private TimeSpan _dynamicAggregationSpan;
        private AggregationLevel _level;
        private readonly CachedCollection<T> _aggregatedData;
        private readonly Func<T, DateTime> _dateSelector;
        private readonly Action<T, T> _aggregator;

        private readonly object _lockObject = new object();
        private int _maxPoints = 0; // Maximum number of points
        private DateTime lastItemDate = DateTime.MinValue;

        //AUTOMATED Aggregation
        private const int WINDOW_SIZE = 10; // Number of items to consider for frequency calculation

        public event EventHandler<int> OnRemoved;
        public event EventHandler<T> OnAdded;

        public AggregatedCollection(IEnumerable<T> items, AggregationLevel level, int maxItems, Func<T, DateTime> dateSelector, Action<T, T> aggregator)
        {
            _aggregatedData = new CachedCollection<T>(items);
            _maxPoints = maxItems;
            _level = level;
            _aggregationSpan = GetAggregationSpan(level);
            _dynamicAggregationSpan = _aggregationSpan; // Initialize with the same value
            _dateSelector = dateSelector;
            _aggregator = aggregator;
        }
        public AggregatedCollection(AggregationLevel level, int maxItems, Func<T, DateTime> dateSelector, Action<T, T> aggregator)
            : this(new List<T>(), level, maxItems, dateSelector, aggregator)
        { }
        ~AggregatedCollection()
        {
            Dispose(false);
        }

        public bool Add(T item)
        {
            lock (_lockObject)
            {
                if (_aggregationSpan == TimeSpan.Zero && _level != AggregationLevel.Automatic)
                {
                    _aggregatedData.Add(item);
                    OnAdded?.Invoke(this, item);
                    while (_aggregatedData.Count() > _maxPoints)
                    {
                        _aggregatedData.RemoveAt(0);
                        OnRemoved?.Invoke(this, 0);
                    }
                    return true;
                }
                else
                {
                    var itemDate = _dateSelector(item);
                    DateTime bucketTime;
                    T existingBucket = default(T);

                    // If AggregationLevel is set to Automatic, adjust the dynamic aggregation span based on recent data frequency.
                    if (_aggregationSpan == GetAggregationSpan(AggregationLevel.Automatic))
                    {
                        _dynamicAggregationSpan = CalculateAutomaticAggregationSpan(itemDate);
                        if (_dynamicAggregationSpan.Ticks > 0)
                        {
                            bucketTime = new DateTime((itemDate.Ticks / _dynamicAggregationSpan.Ticks) * _dynamicAggregationSpan.Ticks);
                            existingBucket = _aggregatedData.FirstOrDefault(p => new DateTime((_dateSelector(p).Ticks / _dynamicAggregationSpan.Ticks) * _dynamicAggregationSpan.Ticks) == bucketTime);
                        }
                    }
                    else
                    {
                        bucketTime = new DateTime((itemDate.Ticks / _aggregationSpan.Ticks) * _aggregationSpan.Ticks);
                        existingBucket = _aggregatedData.FirstOrDefault(p => new DateTime((_dateSelector(p).Ticks / _aggregationSpan.Ticks) * _aggregationSpan.Ticks) == bucketTime);
                    }


                    if (existingBucket != null)
                    {
                        //CALL THE AGGREGATOR
                        if (_aggregator != null)
                        {
                            _aggregator(existingBucket, item);
                        }
                        return false;
                    }
                    else
                    {
                        _aggregatedData.Add(item);
                        OnAdded?.Invoke(this, item);
                        while (_aggregatedData.Count() > _maxPoints)
                        {
                            var itemToRemove = _aggregatedData.FirstOrDefault();
                            if (itemToRemove is IDisposable disposableItem)
                            {
                                disposableItem.Dispose();
                            }
                            _aggregatedData.RemoveAt(0);

                            OnRemoved?.Invoke(this, 0);
                        }
                        return true;
                    }
                }
            }
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
        public IEnumerable<T> ToList()
        {
            lock (_lockObject)
            {
                return _aggregatedData.ToList();
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            lock (_lockObject)
            {
                foreach (var item in _aggregatedData)
                {
                    yield return item;
                }
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
        private TimeSpan GetAggregationSpan(AggregationLevel level)
        {
            switch (level)
            {
                case AggregationLevel.None: return TimeSpan.Zero;
                case AggregationLevel.Ms1: return TimeSpan.FromMilliseconds(1);
                case AggregationLevel.Ms10: return TimeSpan.FromMilliseconds(10);
                case AggregationLevel.Ms100: return TimeSpan.FromMilliseconds(100);
                case AggregationLevel.Ms500: return TimeSpan.FromMilliseconds(500);
                case AggregationLevel.S1: return TimeSpan.FromSeconds(1);
                case AggregationLevel.S3: return TimeSpan.FromSeconds(3);
                case AggregationLevel.S5: return TimeSpan.FromSeconds(5);
                case AggregationLevel.Automatic: return TimeSpan.Zero; // Default behavior for Automatic. It will be recalculated.
                default: throw new ArgumentException("Unsupported aggregation level", nameof(level));
            }
        }
        private TimeSpan CalculateAutomaticAggregationSpan(DateTime currentItemDate)
        {
            if (_aggregatedData.Count() < WINDOW_SIZE)
                return _dynamicAggregationSpan; //return the same
            //var olderItemDate = _dateSelector(_aggregatedData[_aggregatedData.Count - WINDOW_SIZE]);
            var averageElapsed = new TimeSpan((currentItemDate - lastItemDate).Ticks / WINDOW_SIZE);
            lastItemDate = currentItemDate;

            if (averageElapsed <= TimeSpan.FromMilliseconds(1))
                return GetAggregationSpan(AggregationLevel.Ms1);
            else if (averageElapsed <= TimeSpan.FromMilliseconds(10))
                return GetAggregationSpan(AggregationLevel.Ms10);
            else if (averageElapsed <= TimeSpan.FromMilliseconds(100))
                return GetAggregationSpan(AggregationLevel.Ms100);
            else if (averageElapsed <= TimeSpan.FromMilliseconds(500))
                return GetAggregationSpan(AggregationLevel.Ms500);
            else if (averageElapsed <= TimeSpan.FromSeconds(1))
                return GetAggregationSpan(AggregationLevel.S1);
            else if (averageElapsed <= TimeSpan.FromSeconds(3))
                return GetAggregationSpan(AggregationLevel.S3);
            else
                return GetAggregationSpan(AggregationLevel.S5);


        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_aggregatedData != null)
                        _aggregatedData.Clear();
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
