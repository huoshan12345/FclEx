using System;

namespace FclEx.Utils
{
    public readonly struct ProcExItem<T>
    {
        public ProcExItem(T item, Exception exception, int errorTimes)
        {
            Item = item;
            Exception = exception;
            ErrorTimes = errorTimes;
        }

        public T Item { get; }
        public int ErrorTimes { get; }
        public Exception Exception { get; }
    }
}