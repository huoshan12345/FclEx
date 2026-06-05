namespace FclEx.Http;

public class HttpClientOptionsEqualityComparer : IEqualityComparer<HttpClientOptions>
{
    public static readonly HttpClientOptionsEqualityComparer Instance = new();

    private static IEqualityComparer<SocketsHttpHandlerOptions> HandlerOptionsComparer
        => SocketsHttpHandlerOptionsEqualityComparer.Instance;
    private static IEqualityComparer<HttpClientRetryPolicyOptions> RetryPolicyOptionsComparer
        => EqualityComparer<HttpClientRetryPolicyOptions>.Default;

    public bool Equals(HttpClientOptions? x, HttpClientOptions? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

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
            StringComparer.OrdinalIgnoreCase.GetHashCodeOrDefault(obj.BaseAddress?.AbsoluteUri),
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