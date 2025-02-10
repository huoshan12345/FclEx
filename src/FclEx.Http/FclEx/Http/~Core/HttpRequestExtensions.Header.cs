namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddHeader(this HttpRequest request, string key, string? value)
    {
        Check.NotNull(key);
        request.Headers[key.Trim()] = value.ToStringOrEmpty().Trim();
        return request;
    }

    public static HttpRequest TryAddHeader(this HttpRequest request, string key, string? value)
    {
        var k = key.Trim();
        if (!request.Headers.ContainsKey(k))
            request.Headers[k] = value.ToStringOrEmpty().Trim();
        return request;
    }

    public static HttpRequest AddHeaderIfValid(this HttpRequest request, string key, string? value)
    {
        return request.AddHeaderIf(value.IsNotEmpty(), key, value);
    }

    public static HttpRequest AddHeaderIf(this HttpRequest request, bool condition, string key, string? value)
    {
        return condition ? request.AddHeader(key, value) : request;
    }

    public static HttpRequest AddHeader(this HttpRequest request, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras.EmptyIfNull().ForEach(m => request.AddHeader(m));
        return request;
    }

    public static HttpRequest AddHeader(this HttpRequest request, KeyValuePair<string, string?> pair) => request.AddHeader(pair.Key, pair.Value);

    public static HttpRequest AddCookies(this HttpRequest request, string? value)
    {
        return request.AddHeader(HttpKnownHeaderNames.Cookie, value);
    }

    public static HttpRequest AddCookies(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.AddHeader(HttpKnownHeaderNames.Cookie, cookies.JoinWith("; "));
    }

    public static HttpRequest AddHeaderPair(this HttpRequest request, string queryPair, char separator = ':')
    {
        var pair = queryPair.Split(separator);
        request.AddHeader(pair[0], pair.Length > 1 ? pair[1] : "");
        return request;
    }

    public static HttpRequest AcceptUtf8(this HttpRequest request)
    {
        return request.AddHeader("Accept-Charset", "utf-8");
    }

    public static HttpRequest AcceptCn(this HttpRequest request)
    {
        return request.AddHeader("Accept-Language", "zh-CN,zh;q=0.8");
    }

    public static HttpRequest Ajax(this HttpRequest request)
    {
        return request.AddHeader("X-Requested-With", "XMLHttpRequest");
    }

    public static HttpRequest AcceptCompress(this HttpRequest request)
    {
        return request.AddHeader(HttpKnownHeaderNames.AcceptEncoding, "gzip, deflate, br");
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

    public static string GetRequestHeader(this HttpRequest request, string? cookieHeader = null)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in request.Headers)
            sb.AppendLine($"{key}: {value}");
        if (!request.Headers.ContainsKey(HttpKnownHeaderNames.Cookie) && !cookieHeader.IsNullOrEmpty())
            sb.Append(HttpKnownHeaderNames.Cookie + ": " + cookieHeader);
        return sb.ToString();
    }

    public static string GetRequestHeader(this HttpRequest request, IEnumerable<Cookie> cookies)
    {
        return request.GetRequestHeader(cookies.Select(m => m.ToString()).JoinWith("; "));
    }

    public static string GetRequestHeader(this HttpRequest request, CookieCollection cookies)
    {
        return request.GetRequestHeader(cookies.Enumerate());
    }

    public static string GetRequestHeader(this HttpRequest request, CookieContainer cc)
    {
        return request.GetRequestHeader(cc.GetCookies(request.GetUri()));
    }

    public static string GetRequestHeader(this HttpRequest request, IHttpService service)
    {
        return request.GetRequestHeader(service.GetCookies(request.GetUri()));
    }
}