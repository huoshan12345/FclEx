namespace FclEx.Extensions;

public static class CancellationTokenExtensions
{
    public static CancellationTokenSource WithTimeout(this CancellationToken token, TimeSpan? timeout)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (timeout.HasValue)
        {
            cts.CancelAfter(timeout.Value);
        }
        return cts;
    }

    [MethodImpl(AggressiveInlining)]
    public static CancellationTokenRegistration Register<T>(this CancellationToken token, Action<T> callback, T state)
    {
        return token.Register(m => callback((T)m!), state);
    }

    [MethodImpl(AggressiveInlining)]
    public static CancellationTokenRegistration Register<T>(this CancellationToken token, Action<T, CancellationToken> callback, T state)
    {
        return token.Register(m => callback((T)m!, token), state);
    }
}