using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace VisualHFT.Helpers
{
    public class CachedCollection<T> : IEnumerable<T>
    {
        private readonly object _lock = new object();
        private List<T> _internalList;
        private ReadOnlyCollection<T> _cachedReadOnlyCollection;

        public CachedCollection(IEnumerable<T> initialData = null)
        {
            _internalList = initialData?.ToList() ?? new List<T>();
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection == null)
                {
                    _cachedReadOnlyCollection = _internalList.AsReadOnly();
                }
                return _cachedReadOnlyCollection;
            }
        }

        public void Update(IEnumerable<T> newData)
        {
            lock (_lock)
            {
                _internalList = new List<T>(newData);
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
        public bool Remove(Func<T, bool> predicate)
        {
            lock (_lock)
            {
                T itemFound = _internalList.FirstOrDefault(predicate);
                if (itemFound != null)
                {
                    var result = _internalList.Remove(itemFound);
                    if (result)
                    {
                        _cachedReadOnlyCollection = null; // Invalidate the cache
                    }
                    return result;
                }
                return false;
            }
        }
        public void RemoveAt(int index)
        {
            lock (_lock)
            {
                _internalList.RemoveAt(index);
                _cachedReadOnlyCollection = null; // Invalidate the cache
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
        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                if (_cachedReadOnlyCollection != null)
                    return _cachedReadOnlyCollection.ToList().GetEnumerator();
                else
                    return _internalList.ToList().GetEnumerator(); // Create a copy to ensure thread safety during enumeration
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
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
