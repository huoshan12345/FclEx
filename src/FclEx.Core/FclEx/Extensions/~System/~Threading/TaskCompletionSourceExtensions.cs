namespace FclEx.Extensions;

public static class TaskCompletionSourceExtensions
{
#if NET6_0_OR_GREATER
    public static TaskCompletionSource Exception(this TaskCompletionSource source, Exception ex)
    {
        source.SetException(ex.GetBaseException());
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
#endif

    public static TaskCompletionSource<T> Exception<T>(this TaskCompletionSource<T> source, Exception ex)
    {
        source.SetException(ex.GetBaseException());
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
