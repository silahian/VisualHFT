using System.Collections;
using System.Collections.ObjectModel;
using VisualHFT.Commons.Model;
using VisualHFT.Commons.Pools;

namespace VisualHFT.Helpers
{
    public class CachedCollection<T> : IDisposable, IEnumerable<T> where T : class, new()
    {
        private readonly object _lock = new object();
        private List<T> _internalList;
        private List<T> _cachedReadOnlyCollection;
        private CachedCollection<T> _takeList;
        private Comparison<T> _comparison;

        public CachedCollection(IEnumerable<T> initialData = null)
        {
            _internalList = initialData?.ToList() ?? new List<T>();
        }
        public CachedCollection(Comparison<T> comparison = null, int listSize = 0)
        {
            if (listSize > 0)
                _internalList = new List<T>(listSize);
            else
                _internalList = new List<T>();
            _comparison = comparison;
        }

        public CachedCollection(IEnumerable<T> initialData = null, Comparison<T> comparison = null)
        {
            _internalList = initialData?.ToList() ?? new List<T>();
            _comparison = comparison;
            if (_comparison != null)
            {
                _internalList.Sort(_comparison);
            }
        }


        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                {
                    return _cachedReadOnlyCollection.AsReadOnly();
                }
                else
                    return _internalList.AsReadOnly();
            }
        }

        public void Update(IEnumerable<T> newData)
        {
            lock (_lock)
            {
                _internalList = new List<T>(newData);
                Sort();
                _cachedReadOnlyCollection = null; // Invalidate the cache
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _internalList.Clear();
                _cachedReadOnlyCollection = null; // Invalidate the cache
            }
        }
        public int Count()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.Count;
                else
                    return _internalList.Count;
            }
        }
        public void Add(T item)
        {
            lock (_lock)
            {
                _internalList.Add(item);
                Sort();
                _cachedReadOnlyCollection = null; // Invalidate the cache
            }
        }
        public bool Remove(T item)
        {
            lock (_lock)
            {
                var result = _internalList.Remove(item);
                if (result)
                {
                    _cachedReadOnlyCollection = null; // Invalidate the cache
                }
                return result;
            }
        }
        public bool RemoveAll(Predicate<T> predicate)
        {
            return Remove(predicate);
        }
        public bool Remove(Predicate<T> predicate)
        {
            lock (_lock)
            {
                bool removed = false;
                for (int i = _internalList.Count - 1; i >= 0; i--)
                {
                    if (predicate(_internalList[i]))
                    {
                        var item = _internalList[i];
                        _internalList.RemoveAt(i);
                        removed = true;
                    }
                }
                if (removed)
                {
                    _cachedReadOnlyCollection = null; // Invalidate the cache
                }
                return removed;
            }
        }

        public T FirstOrDefault()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.FirstOrDefault();
                else
                    return _internalList.FirstOrDefault();
            }
        }
        public T FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.FirstOrDefault(predicate);
                else
                    return _internalList.FirstOrDefault(predicate);
            }
        }


        public CachedCollection<T> Take(int count)
        {
            lock (_lock)
            {
                if (count <= 0)
                {
                    return null;
                }

                if (_takeList == null)
                    _takeList = new CachedCollection<T>(_comparison);

                _takeList.Clear();
                if (_cachedReadOnlyCollection != null)
                {
                    for (int i = 0; i < Math.Min(count, _cachedReadOnlyCollection.Count); i++)
                    {
                        _takeList.Add(_cachedReadOnlyCollection[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < Math.Min(count, _internalList.Count); i++)
                    {
                        _takeList.Add(_internalList[i]);
                    }
                }

                return _takeList;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.GetEnumerator();
                else
                    return _internalList.GetEnumerator(); // Create a copy to ensure thread safety during enumeration
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private class TakeEnumerable : IEnumerable<T>
        {
            private readonly List<T> _source;
            private readonly int _count;

            public TakeEnumerable(List<T> source, int count)
            {
                _source = source;
                _count = count;
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (int i = 0; i < _count && i < _source.Count; i++)
                {
                    yield return _source[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            var coll = (obj as CachedCollection<T>);
            if (coll == null)
                return false;

            for (int i = 0; i < coll.Count(); i++)
            {
                if (!_internalList[i].Equals(coll[i]))
                    return false;
            }


            return true;
        }

        public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    if (_cachedReadOnlyCollection != null)
                    {
                        return _cachedReadOnlyCollection[index];
                    }
                    else
                    {
                        return _internalList[index];
                    }
                }
            }

        }
        public bool Update(Func<T, bool> predicate, Action<T> actionUpdate)
        {
            lock (_lock)
            {
                T itemFound = _internalList.FirstOrDefault(predicate);
                if (itemFound != null)
                {
                    //execute actionUpdate
                    actionUpdate(itemFound);
                    Sort();
                    InvalidateCache();
                    return true;
                }

                return false;
            }
        }
        public long IndexOf(T element)
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.IndexOf(element);
                else
                    return _internalList.IndexOf(element);
            }
        }
        public void InvalidateCache()
        {
            _cachedReadOnlyCollection = null; // Invalidate the cache
        }
        public void Sort()
        {
            lock (_lock)
            {
                if (_comparison != null)
                {
                    _internalList.Sort(_comparison);
                    InvalidateCache();
                }
            }
        }

        public void ShallowCopyFrom(CachedCollection<T> sourceList)
        {
            ShallowCopyFrom(sourceList, null);
        }
        public void ShallowCopyFrom(CachedCollection<T> sourceList, CustomObjectPool<T> pool = null)
        {
            if (sourceList == null)
                return;
            lock (_lock)
            {
                Clear();
                if (sourceList.Count() > _internalList.Count) //add empty items to match the source list
                {
                    for (int i = 0; i < sourceList.Count(); i++)
                    {
                        if (pool != null)
                            _internalList.Add(pool.Get());
                        else
                            _internalList.Add(new T());
                    }
                }

                for (int i = 0; i < _internalList.Count; i++)
                {
                    if (i < sourceList.Count())
                    {
                        if (!_internalList[i].Equals(sourceList[i]))
                            (_internalList[i] as ICopiable<T>)?.CopyFrom(sourceList[i]);
                    }
                    else
                        (_internalList[i] as IResettable)?.Reset();
                }
                Sort();
            }
        }
        /// <summary>
        /// ShallowUpdateFrom
        /// Will update an existing list that will never change.
        /// This is very useful when keeping a Collection locally and want to avoid swapping and allocations
        /// One place that is being used is in vmOrderBook, to keep the Grids updated.
        /// </summary>
        /// <param name="sourceList">The source list.</param>
        public void ShallowUpdateFrom(CachedCollection<T> sourceList)
        {
            if (sourceList == null)
                return;
            lock (_lock)
            {
                if (sourceList.Count() > _internalList.Count) //add empty items to match the source list
                {
                    for (int i = 0; i < sourceList.Count() - _internalList.Count + 1; i++)
                    {
                        _internalList.Add(new T());
                    }
                }

                for (int i = 0; i < _internalList.Count; i++)
                {
                    if (i < sourceList.Count())
                    {
                        if (!_internalList[i].Equals(sourceList[i]))
                            (_internalList[i] as ICopiable<T>)?.CopyFrom(sourceList[i]);
                    }
                    else
                        (_internalList[i] as IResettable)?.Reset();
                }
                Sort();
            }
        }

        public void Dispose()
        {
            _internalList.Clear();
            _cachedReadOnlyCollection?.Clear();
            _takeList.Dispose();
        }
    }
}
