using Microsoft.Extensions.ObjectPool;

namespace VisualHFT.Commons.Pools
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly DefaultObjectPool<T> _pool;

        public ObjectPool()
        {
            _pool = new DefaultObjectPool<T>(new DefaultPooledObjectPolicy<T>());
        }

        public T Get()
        {
            return _pool.Get();
        }

        public void Return(T obj)
        {
            _pool.Return(obj);            
        }
    }

}
