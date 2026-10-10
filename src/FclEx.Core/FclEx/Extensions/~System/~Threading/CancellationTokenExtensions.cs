namespace FclEx.Extensions;

public static class CancellationTokenExtensions
{
    public static CancellationTokenSource WithTimeout(this CancellationToken cancellationToken, TimeSpan? timeout)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout.HasValue)
        {
            cts.CancelAfter(timeout.Value);
        }
        return cts;
    }

    [MethodImpl(AggressiveInlining)]
    public static CancellationTokenRegistration Register<T>(this CancellationToken cancellationToken, Action<T> callback, T state)
    {
        return cancellationToken.Register(m => callback((T)m!), state);
    }

    [MethodImpl(AggressiveInlining)]
    public static CancellationTokenRegistration Register<T>(this CancellationToken cancellationToken, Action<T, CancellationToken> callback, T state)
    {
        return cancellationToken.Register(m => callback((T)m!, cancellationToken), state);
    }
}