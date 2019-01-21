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

    public class ProcItem<T>
    {
        public ProcItem(T item)
        {
            Item = item;
            ErrorTimes = 0;
            Exception = null;
        }

        public int ErrorTimes { get; private set; }
        public T Item { get; }
        public Exception Exception { get; private set; }

        internal void AddError(Exception ex)
        {
            Exception = ex;
            ++ErrorTimes;
        }
    }
}
