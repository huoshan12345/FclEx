using System;

namespace FclEx.Utils
{
    public static class ProcItem
    {
        public static ProcItem<T> Create<T>(T item)
        {
            return new ProcItem<T>(item);
        }
    }

    public struct ProcItem<T>
    {
        public ProcItem(T item)
        {
            Item = item;
            ErrorTimes = 0;
            Exception = null;
        }

        public int ErrorTimes { get; private set; }
        public T Item { get; }
        public Exception? Exception { get; private set; }

        public ProcItem<T> AddError(Exception ex)
        {
            return new ProcItem<T>(Item) { ErrorTimes = ErrorTimes + 1, Exception = ex };
        }

        public ProcItem<T1> ToType<T1>(T1 item)
        {
            return new ProcItem<T1>(item) { ErrorTimes = ErrorTimes, Exception = Exception };
        }
    }

    public static class ProcItemExtensions
    {
        public static bool HasError<T>(this ProcItem<T> item)
        {
            return item.ErrorTimes > 0;
        }
    }
}
