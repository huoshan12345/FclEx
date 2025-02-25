namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddHeader(this HttpRequest request, string key, string? value)
    {
        Check.NotNull(key);
        request.Headers.Add(key, value);
        return request;
    }

    public static HttpRequest SetHeader(this HttpRequest request, string key, string? value)
    {
        Check.NotNull(key);
        request.Headers.Set(key, value);
        return request;
    }

    public static HttpRequest TryAddHeader(this HttpRequest request, string key, string? value)
    {
        if (request.Headers.ContainsKey(key) == false)
        {
            request.AddHeader(key, value);
        }
        return request;
    }

    public static HttpRequest TrySetHeader(this HttpRequest request, string key, string? value)
    {
        if (request.Headers.ContainsKey(key) == false)
        {
            request.SetHeader(key, value);
        }
        return request;
    }

    public static HttpRequest AddHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        foreach (var (key, value) in paras.EmptyIfNull())
        {
            return request.AddHeader(key, value);
        }
        return request;
    }

    public static HttpRequest AddHeader<T>(this HttpRequest request, IEnumerable<KeyValuePair<string, T>> paras) where T : IEnumerable<string?>
    {
        foreach (var (key, values) in paras.EmptyIfNull())
        {
            foreach (var value in values)
            {
                return request.AddHeader(key, value);
            }
        }

        return request;
    }

    public static HttpRequest AddHeader(this HttpRequest request, KeyValuePair<string, string?> pair)
    {
        return request.AddHeader(pair.Key, pair.Value);
    }

    public static HttpRequest AddCookies(this HttpRequest request, string? value)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, value);
    }

    public static HttpRequest AddCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.AddHeader(HttpHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    public static HttpRequest AddHeaderPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        request.AddHeader(pair[0], pair.Length > 1 ? pair[1] : "");
        return request;
    }

    public static HttpRequest AcceptUtf8(this HttpRequest request)
    {
        return request.AddHeader(HttpHeaderNames.AcceptCharset, "utf-8");
    }

    public static HttpRequest AcceptCn(this HttpRequest request)
    {
        return request.AddHeader(HttpHeaderNames.AcceptLanguage, "zh-CN,zh;q=0.8");
    }

    public static HttpRequest Ajax(this HttpRequest request)
    {
        return request.AddHeader("X-Requested-With", "XMLHttpRequest");
    }

    public static HttpRequest AcceptCompress(this HttpRequest request)
    {
        return request.AddHeader(HttpHeaderNames.AcceptEncoding, "gzip");
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