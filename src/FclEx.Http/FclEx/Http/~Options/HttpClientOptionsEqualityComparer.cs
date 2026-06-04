namespace FclEx.Http;

public class HttpClientOptionsEqualityComparer : IEqualityComparer<HttpClientOptions>
{
    public static readonly HttpClientOptionsEqualityComparer Instance = new();
    private static readonly IEqualityComparer<SocketsHttpHandlerOptions> HandlerOptionsComparer
        = SocketsHttpHandlerOptionsEqualityComparer.Instance;
    private static readonly IEqualityComparer<HttpClientRetryPolicyOptions> RetryPolicyOptionsComparer
    = EqualityComparer<HttpClientRetryPolicyOptions>.Default;

    public bool Equals(HttpClientOptions? x, HttpClientOptions? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;

        return HandlerOptionsComparer.Equals(x.HandlerOptions, y.HandlerOptions)
               && RetryPolicyOptionsComparer.Equals(x.RetryPolicyOptions, y.RetryPolicyOptions)
#if NET6_0_OR_GREATER
               && x.HttpVersionPolicy == y.HttpVersionPolicy
               && x.HttpVersion.Equals(y.HttpVersion)
#endif
               && Uri.Compare(x.BaseAddress, y.BaseAddress, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0
               && x.TotalTimeout.Equals(y.TotalTimeout);
    }

    public int GetHashCode(HttpClientOptions obj)
    {
        var code = HashCode.Combine(
            obj.BaseAddress?.AbsoluteUri,
#if NET6_0_OR_GREATER
            obj.HttpVersion,
            obj.HttpVersionPolicy,
#endif
            obj.TotalTimeout);

        return HashCode.Combine(
            HandlerOptionsComparer.GetHashCode(obj.HandlerOptions),
            RetryPolicyOptionsComparer.GetHashCode(obj.RetryPolicyOptions),
            code);
    }
}