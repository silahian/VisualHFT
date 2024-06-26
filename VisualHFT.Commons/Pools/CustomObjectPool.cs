using Microsoft.Extensions.ObjectPool;
using System.Collections;
using System.Diagnostics;
using VisualHFT.Commons.Model;

namespace VisualHFT.Commons.Pools
{
    public class CustomObjectPool<T> : IDisposable where T : class, new()
    {
        private DefaultObjectPool<T> _pool;
        private int _maxPoolSize = 0;
        private int _availableObjects;
        private bool _disposed = false;
        private double _utilizationPercentage;
        private DateTime _lastUpdateLog = DateTime.MinValue;
        private string _poolName;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CustomObjectPool(/*string poolName, */int maxPoolSize = 100)
        {
            //_poolName = poolName;
            _maxPoolSize = maxPoolSize;
            _pool = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy<T>(), _maxPoolSize);
            _availableObjects = _maxPoolSize;
            _utilizationPercentage = 0;
        }

        public T Get()
        {
            Interlocked.Decrement(ref _availableObjects);
            CalculatePercentageUtilization();
            return _pool.Get();
        }

        public void Return(IEnumerable<T> listObjs)
        {
            if (listObjs == null)
                return;
            foreach (var obj in listObjs)
            {
                Return(obj);
            }
        }
        public void Return(T obj)
        {
            (obj as VisualHFT.Commons.Model.IResettable)?.Reset();
            (obj as IList)?.Clear();
            _pool.Return(obj);
            Interlocked.Increment(ref _availableObjects);
            CalculatePercentageUtilization();
        }
        public int AvailableObjects => _availableObjects;
        public string? ProviderName { get; set; }

        private void CalculatePercentageUtilization()
        {
            if (_maxPoolSize > 0)
                _utilizationPercentage = 1.0 - (_availableObjects / (double)_maxPoolSize);
            if (_availableObjects < 0) //is being overused
            {
                if (DateTime.Now.Subtract(_lastUpdateLog).TotalSeconds > 5)
                {
                    var typeName = typeof(T).Name;
                    var stackTrace = new StackTrace();
                    var callingMethod = stackTrace.GetFrame(2).GetMethod();
                    var caller = callingMethod.ReflectedType != null
                        ? callingMethod.ReflectedType.Name
                        : "Unknown" + "." + callingMethod.Name;

                    log.Warn($"CustomObjectPool<{typeName}>: called by {caller} {ProviderName} - utilization: {_utilizationPercentage.ToString("p2")}");
                    _lastUpdateLog = DateTime.Now;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }

                _disposed = true;
            }
        }
    }


}
