namespace FclEx.Http;

partial class HttpRequestExtensions
{
    /// <summary>
    /// Adds a header value to the request header collection.
    /// Existing values for the same header are preserved.
    /// </summary>
    public static HttpRequest AddHeader(this HttpRequest request, string key, string? value)
    {
        request.Headers.Add(key, value);
        return request;
    }

    /// <summary>
    /// Replaces existing values for a request header.
    /// </summary>
    public static HttpRequest SetHeader(this HttpRequest request, string key, string? value)
    {
        request.Headers.Set(key, value);
        return request;
    }

    /// <summary>
    /// Removes all values for a request header.
    /// </summary>
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

    /// <summary>
    /// Adds multiple header values while preserving existing values for the same header names.
    /// </summary>
    public static HttpRequest AddHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Headers.Add(pairs);
        return request;
    }

    /// <summary>
    /// Replaces header values from multiple name-value pairs.
    /// </summary>
    public static HttpRequest SetHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string>> pairs)
    {
        request.Headers.Set(pairs);
        return request;
    }

    /// <summary>
    /// Adds multiple multi-value headers while preserving existing values for the same header names.
    /// </summary>
    public static HttpRequest AddHeader<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Headers.Add(pairs);
        return request;
    }

    /// <summary>
    /// Replaces multiple multi-value headers.
    /// </summary>
    public static HttpRequest SetHeader<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> pairs)
        where T : IEnumerable<string>
    {
        request.Headers.Set(pairs);
        return request;
    }

    /// <summary>
    /// Adds a raw Cookie header value.
    /// </summary>
    public static HttpRequest AddCookies(this HttpRequest request, string? value)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, value);
    }

    /// <summary>
    /// Replaces the request Cookie header with a raw value.
    /// </summary>
    public static HttpRequest SetCookies(this HttpRequest request, string? value)
    {
        return request.SetHeader(HttpHeaderNames.Cookie, value);
    }

    /// <summary>
    /// Adds cookies as one Cookie header line joined with semicolons.
    /// </summary>
    public static HttpRequest AddCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    /// <summary>
    /// Replaces the request Cookie header with cookies joined by semicolons.
    /// </summary>
    public static HttpRequest SetCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.SetHeader(HttpHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    /// <summary>
    /// Parses a single header line into name and value and adds it to the request.
    /// The first occurrence of <paramref name="separator"/> splits the line.
    /// </summary>
    public static HttpRequest AddHeaderLine(this HttpRequest request, string pair, string separator = ":")
    {
        Check.NotEmpty(pair);
        Check.NotEmpty(separator);

        var (key, value) = pair.Partition(separator);
        request.AddHeader(key, value);
        return request;
    }

    /// <summary>
    /// Sets the Accept-Charset header to UTF-8.
    /// </summary>
    public static HttpRequest AcceptUtf8(this HttpRequest request)
    {
        return request.SetHeader(HttpHeaderNames.AcceptCharset, "utf-8");
    }

    /// <summary>
    /// Sets the Accept-Language header to prefer Simplified Chinese.
    /// </summary>
    public static HttpRequest AcceptChinese(this HttpRequest request)
    {
        return request.SetHeader(HttpHeaderNames.AcceptLanguage, "zh-CN,zh;q=0.8");
    }

    /// <summary>
    /// Marks the request as an XMLHttpRequest by setting X-Requested-With.
    /// </summary>
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

    /// <summary>
    /// Sets the Accept-Encoding header.
    /// When no encodings are supplied, the default list follows the compression algorithms available on the target framework.
    /// </summary>
    public static HttpRequest AcceptCompress(this HttpRequest request, IEnumerable<string>? encodings = null)
    {
        return request.SetHeader(HttpHeaderNames.AcceptEncoding, string.Join(", ", encodings ?? _defaultEncodings));
    }

    /// <summary>
    /// Enables or disables GZip compression for outgoing request content.
    /// </summary>
    public static HttpRequest UseGZip(this HttpRequest request, bool gzip = true, CompressionLevel level = CompressionLevel.Optimal)
    {
        return request.Compression(gzip ? CompressionMethod.GZip : CompressionMethod.None, level);
    }

    /// <summary>
    /// Sets the compression method and level used when outgoing request content is serialized.
    /// </summary>
    public static HttpRequest Compression(this HttpRequest request, CompressionMethod method, CompressionLevel level = CompressionLevel.Optimal)
    {
        request.CompressionMethod = method;
        request.CompressionLevel = level;
        return request;
    }

    /// <summary>
    /// Sets the Referrer header shortcut.
    /// </summary>
    public static HttpRequest Referrer(this HttpRequest request, string? referrer)
    {
        request.Referrer = referrer;
        return request;
    }

    /// <summary>
    /// Sets the Referrer header shortcut only when it is currently unset.
    /// </summary>
    public static HttpRequest TryReferrer(this HttpRequest request, string? referrer)
    {
        request.Referrer ??= referrer;
        return request;
    }

    /// <summary>
    /// Sets the User-Agent header shortcut.
    /// </summary>
    public static HttpRequest UserAgent(this HttpRequest request, string? userAgent)
    {
        request.UserAgent = userAgent;
        return request;
    }

    /// <summary>
    /// Sets the User-Agent header shortcut only when it is currently unset.
    /// </summary>
    public static HttpRequest TryUserAgent(this HttpRequest request, string? userAgent)
    {
        request.UserAgent ??= userAgent;
        return request;
    }
}
