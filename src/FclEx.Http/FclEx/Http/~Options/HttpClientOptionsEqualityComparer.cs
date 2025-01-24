namespace FclEx.Http;

public class HttpClientOptionsEqualityComparer : IEqualityComparer<HttpClientOptions>
{
    public static readonly HttpClientOptionsEqualityComparer Instance = new();
    private static readonly IEqualityComparer<SocketsHttpHandlerOptions> BaseComparer
        = SocketsHttpHandlerOptionsEqualityComparer.Instance;

    public bool Equals(HttpClientOptions? x, HttpClientOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;

        return BaseComparer.Equals(x, y)
#if NET6_0_OR_GREATER
               && x.HttpVersionPolicy == y.HttpVersionPolicy
               && x.HttpVersion.Equals(y.HttpVersion)
#endif
               && x.ExecutionTimeout.Equals(y.ExecutionTimeout)
               && Uri.Compare(x.BaseAddress, y.BaseAddress, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0
               && x.RetryCount == y.RetryCount
               && x.AutoUpdateTotalTimeout == y.AutoUpdateTotalTimeout
               && x.SleepDurationProvider.Equals(y.SleepDurationProvider)
               && x.TotalTimeout.Equals(y.TotalTimeout);
    }

    public int GetHashCode(HttpClientOptions obj)
    {
        var code = HashCode.Combine(
            obj.BaseAddress?.AbsoluteUri,
            obj.ExecutionTimeout,
#if NET6_0_OR_GREATER
            obj.HttpVersion,
            obj.HttpVersionPolicy,
#endif
            obj.RetryCount,
            obj.AutoUpdateTotalTimeout,
            obj.SleepDurationProvider,
            obj.TotalTimeout);

        return HashCode.Combine(BaseComparer.GetHashCode(obj), code);
    }
}