namespace FclEx.Extensions;

// ReSharper disable once PartialTypeWithSinglePart
public static partial class AsyncEventHandlerExtensions
{
    public static T[] GetInvocationList<T>(this T @delegate) where T : Delegate
    {
        // cannot use explicit cast here cause System.InvalidCastException will be raised.
        return Unsafe.As<T[]>(@delegate.GetInvocationList());
    }

    public static Task InvokeAsync<TSender>(this AsyncEventHandler<TSender> handler, TSender sender)
    {
        return handler
            .GetInvocationList<AsyncEventHandler<TSender>>()
            .Select(m => m(sender))
            .WhenAll();
    }
}