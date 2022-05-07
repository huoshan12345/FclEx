using System.Buffers;
using FclEx.Utils;

namespace FclEx
{
    public static class ArrayPoolExtensions
    {
        public static PooledArray<T> GetAsDisposable<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
        {
            return new PooledArray<T>(pool, minimumLength, clearArray);
        }
    }
}
