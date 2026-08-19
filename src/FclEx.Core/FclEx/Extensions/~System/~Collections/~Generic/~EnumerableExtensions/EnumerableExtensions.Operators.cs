namespace FclEx.Extensions;

public static partial class EnumerableExtensions
{
    extension<T>(IEnumerable<T>)
    {
        public static IEnumerable<T> operator +(IEnumerable<T> enumerable, IEnumerable<T> other)
        {
            return enumerable.Concat(other);
        }

        public static IEnumerable<T> operator +(IEnumerable<T> enumerable, T item)
        {
            return enumerable.Append(item);
        }

        public static IEnumerable<T> operator +(T item, IEnumerable<T> enumerable)
        {
            return enumerable.Prepend(item);
        }
    }
}
