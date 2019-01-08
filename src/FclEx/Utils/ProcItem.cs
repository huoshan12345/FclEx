using System;

namespace FclEx.Utils
{
    public static class ProcItem
    {
        public static ProcItem<T> Create<T>(T item, int errorTimes = 0)
        {
            return new ProcItem<T>(item, errorTimes);
        }

        public static ProcExItem<T> CreateEx<T>(T item, Exception exception, int errorTimes)
        {
            return new ProcExItem<T>(item, exception, errorTimes);
        }

        public static ProcExItem<T> CreateEx<T>(ProcItem<T> item)
        {
            return new ProcExItem<T>(item.Item, item.LastEx, item.ErrorTimes);
        }
    }

    public struct ProcItem<T>
    {
        public ProcItem(T item, int errorTimes = 0)
        {
            Item = item;
            ErrorTimes = errorTimes;
            LastEx = null;
        }

        public int ErrorTimes { get; set; }
        public T Item { get; }
        public Exception LastEx { get; set; }
    }
}
