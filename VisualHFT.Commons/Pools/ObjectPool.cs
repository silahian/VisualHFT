using Microsoft.Extensions.ObjectPool;
using System.ComponentModel.DataAnnotations;
using VisualHFT.Commons.Model;

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
            (obj as IResettable)?.Reset();

            _pool.Return(obj);
        }
    }

}
