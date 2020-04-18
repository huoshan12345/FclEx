using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Utils
{
    public readonly struct CompositeDisposable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _enumerable;

        public CompositeDisposable(IEnumerable<IDisposable> enumerable)
        {
            _enumerable = enumerable;
        }


        public void Dispose()
        {
            foreach (var e in _enumerable.Touch())
                e?.Dispose();
        }
    }

    public readonly struct CompositeDisposable<T> : IDisposable where T : IDisposable
    {
        private readonly IEnumerable<T> _enumerable;

        public CompositeDisposable(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
        }

        public void Dispose()
        {
            foreach (var e in _enumerable.Touch())
                e?.Dispose();
        }
    }

    public static class Extensions
    {
        public static CompositeDisposable AsComposite(this IEnumerable<IDisposable> enumerable)
        {
            return new CompositeDisposable(enumerable);
        }

        public static CompositeDisposable<T> AsComposite<T>(this IEnumerable<T> enumerable) where T : IDisposable
        {
            return new CompositeDisposable<T>(enumerable);
        }
    }
}
