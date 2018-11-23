using System.Threading;

namespace FclEx.Utils
{
    public class Counter
    {
        private int _count;

        public Counter(int count = 0)
        {
            _count = count;
        }

        public int Count => _count;

        public void Increment() => Interlocked.Increment(ref _count);
        public void Decrement() => Interlocked.Decrement(ref _count);
        public void Add(int value) => Interlocked.Add(ref _count, value);
    }
}
