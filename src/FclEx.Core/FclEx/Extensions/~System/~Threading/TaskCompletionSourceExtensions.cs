namespace FclEx.Extensions;

public static class TaskCompletionSourceExtensions
{
    public static TaskCompletionSource Exception(this TaskCompletionSource source, Exception ex)
    {
        source.SetException(ex);
        return source;
    }

    public static TaskCompletionSource Canceled(this TaskCompletionSource source)
    {
        source.SetCanceled();
        return source;
    }

    public static TaskCompletionSource Result(this TaskCompletionSource source)
    {
        source.SetResult();
        return source;
    }

    public static TaskCompletionSource<T> Exception<T>(this TaskCompletionSource<T> source, Exception ex)
    {
        source.SetException(ex);
        return source;
    }

    public static TaskCompletionSource<T> Canceled<T>(this TaskCompletionSource<T> source)
    {
        source.SetCanceled();
        return source;
    }

    public static TaskCompletionSource<T> Result<T>(this TaskCompletionSource<T> source, T result)
    {
        source.SetResult(result);
        return source;
    }
}
