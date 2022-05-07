using System;
using Dawn;

namespace Microsoft.Extensions.ObjectPool
{
    public struct PooledObject<T> : IDisposable where T : class
    {
        private readonly T _value;
        private bool _isDisposed;
        private readonly ObjectPool<T> _pool;

        public PooledObject(ObjectPool<T> pool)
        {
            _pool = Guard.Argument(pool, nameof(pool)).NotNull();
            _value = pool.Get();
            _isDisposed = false;
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("The object has been disposed already.");
        }

        public T Value
        {
            get
            {
                CheckDisposed();
                return _value;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _pool.Return(_value);
            _isDisposed = true;
        }
    }
}
