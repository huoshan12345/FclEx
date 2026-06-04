namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddHeader(this HttpRequest request, string key, string? value)
    {
        request.Headers.Add(key, value);
        return request;
    }

    public static HttpRequest SetHeader(this HttpRequest request, string key, string? value)
    {
        request.Headers.Set(key, value);
        return request;
    }

    public static HttpRequest RemoveHeader(this HttpRequest request, string key)
    {
        request.Headers.Remove(key);
        return request;
    }

    /// <summary>
    /// Adds a header only when the request does not already contain the header key.
    /// </summary>
    /// <remarks>
    /// The <c>Try</c> prefix follows the Microsoft.Extensions.DependencyInjection <c>TryAdd*</c> convention:
    /// the method is conditional, but still returns the request for fluent chaining.
    /// </remarks>
    public static HttpRequest TryAddHeader(this HttpRequest request, string key, string? value)
    {
        if (request.Headers.ContainsKey(key) == false)
        {
            request.AddHeader(key, value);
        }
        return request;
    }

    /// <summary>
    /// Sets a header only when the request does not already contain the header key.
    /// </summary>
    /// <remarks>
    /// The <c>Try</c> prefix follows the Microsoft.Extensions.DependencyInjection <c>TryAdd*</c> convention:
    /// the method is conditional, but still returns the request for fluent chaining.
    /// </remarks>
    public static HttpRequest TrySetHeader(this HttpRequest request, string key, string? value)
    {
        if (request.Headers.ContainsKey(key) == false)
        {
            request.SetHeader(key, value);
        }
        return request;
    }

    public static HttpRequest AddHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Headers.Add(pairs);
        return request;
    }

    public static HttpRequest SetHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Headers.Set(pairs);
        return request;
    }

    public static HttpRequest AddHeader<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Headers.Add(pairs);
        return request;
    }

    public static HttpRequest SetHeader<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Headers.Set(pairs);
        return request;
    }

    public static HttpRequest AddCookies(this HttpRequest request, string? value)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, value);
    }

    public static HttpRequest SetCookies(this HttpRequest request, string? value)
    {
        return request.SetHeader(HttpHeaderNames.Cookie, value);
    }

    public static HttpRequest AddCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    public static HttpRequest SetCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.SetHeader(HttpHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    public static HttpRequest AddHeaderLine(this HttpRequest request, string pair, string separator = ":")
    {
        Check.NotEmpty(pair);
        var (key, value) = pair.Partition(separator.ToString());
        request.AddHeader(key, value);
        return request;
    }

    public static HttpRequest AcceptUtf8(this HttpRequest request)
    {
        return request.SetHeader(HttpHeaderNames.AcceptCharset, "utf-8");
    }

    public static HttpRequest AcceptCn(this HttpRequest request)
    {
        return request.SetHeader(HttpHeaderNames.AcceptLanguage, "zh-CN,zh;q=0.8");
    }

    public static HttpRequest Ajax(this HttpRequest request)
    {
        return request.SetHeader(HttpHeaderNames.XRequestedWith, "XMLHttpRequest");
    }

    private static readonly string[] _defaultEncodings =
#if NET5_0_OR_GREATER
        ["gzip", "deflate", "br"];
#else
        ["gzip"];
#endif

    public static HttpRequest AcceptCompress(this HttpRequest request, IEnumerable<string>? encodings = null)
    {
        return request.SetHeader(HttpHeaderNames.AcceptEncoding, string.Join(", ", encodings ?? _defaultEncodings));
    }

    public static HttpRequest UseGZip(this HttpRequest request, bool gzip = true, CompressionLevel level = CompressionLevel.Optimal)
    {
        return request.Compression(gzip ? CompressionMethod.GZip : CompressionMethod.None, level);
    }

    public static HttpRequest Compression(this HttpRequest request, CompressionMethod method, CompressionLevel level = CompressionLevel.Optimal)
    {
        request.CompressionMethod = method;
        request.CompressionLevel = level;
        return request;
    }

    public static HttpRequest Referrer(this HttpRequest request, string? referrer)
    {
        request.Referrer = referrer;
        return request;
    }

    public static HttpRequest TryReferrer(this HttpRequest request, string? referrer)
    {
        request.Referrer ??= referrer;
        return request;
    }

    public static HttpRequest UserAgent(this HttpRequest request, string? userAgent)
    {
        request.UserAgent = userAgent;
        return request;
    }

    public static HttpRequest TryUserAgent(this HttpRequest request, string? userAgent)
    {
        request.UserAgent ??= userAgent;
        return request;
    }
}
