namespace FclEx.Utils;

public static class ProcessingItem
{
    public static ProcessingItem<T> Create<T>(T item)
    {
        return new ProcessingItem<T>(item);
    }
}

public readonly record struct ProcessingItem<T>(T Item, int ErrorTimes = 0, Exception? Exception = null)
{
    public ProcessingItem<T> AddError(Exception ex)
    {
        return new ProcessingItem<T>(Item) { ErrorTimes = ErrorTimes + 1, Exception = ex };
    }

    public ProcessingItem<T1> ToType<T1>(T1 item)
    {
        return new ProcessingItem<T1>(item) { ErrorTimes = ErrorTimes, Exception = Exception };
    }

    public bool Error => ErrorTimes > 0;
}