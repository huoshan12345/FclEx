using MoreLinq;

namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest AddHeader(this HttpRequest req, string key, string? value)
    {
        Check.NotNull(key);
        req.Headers[key.Trim()] = value.ToStringOrEmpty().Trim();
        return req;
    }

    public static HttpRequest TryAddHeader(this HttpRequest req, string key, string? value)
    {
        var k = key.Trim();
        if (!req.Headers.ContainsKey(k))
            req.Headers[k] = value.ToStringOrEmpty().Trim();
        return req;
    }

    public static HttpRequest AddHeaderIfValid(this HttpRequest req, string key, string? value)
    {
        return req.AddHeaderIf(value.IsValid(), key, value);
    }

    public static HttpRequest AddHeaderIf(this HttpRequest req, bool condition, string key, string? value)
    {
        return condition ? req.AddHeader(key, value) : req;
    }

    public static HttpRequest AddHeader(this HttpRequest req, IEnumerable<KeyValuePair<string, string?>> paras)
    {
        paras.EmptyIfNull().ForEach(m => req.AddHeader(m));
        return req;
    }

    public static HttpRequest AddHeader(this HttpRequest req, KeyValuePair<string, string?> pair) => req.AddHeader(pair.Key, pair.Value);

    public static HttpRequest AddHeaderPair(this HttpRequest req, string queryPair, char sepetator = ':')
    {
        var pair = queryPair.Split(sepetator);
        req.AddHeader(pair[0], pair.Length > 1 ? pair[1] : "");
        return req;
    }

    public static HttpRequest AcceptUtf8(this HttpRequest req)
    {
        return req.AddHeader("Accept-Charset", "utf-8");
    }

    public static HttpRequest AcceptCn(this HttpRequest req)
    {
        return req.AddHeader("Accept-Language", "zh-CN,zh;q=0.8");
    }

    public static HttpRequest Ajax(this HttpRequest req)
    {
        return req.AddHeader("X-Requested-With", "XMLHttpRequest");
    }

    public static HttpRequest AcceptCompress(this HttpRequest req)
    {
        return req.AddHeader(HttpKnownHeaderNames.AcceptEncoding, "gzip, deflate, br");
    }

    public static HttpRequest UseGZip(this HttpRequest req, bool gzip = true, CompressionLevel level = CompressionLevel.SmallestSize)
    {
        return req.Compression(gzip ? CompressionMethod.GZip : CompressionMethod.None, level);
    }

    public static HttpRequest Compression(this HttpRequest req, CompressionMethod method, CompressionLevel level = CompressionLevel.SmallestSize)
    {
        req.CompressionMethod = method;
        req.CompressionLevel = level;
        return req;
    }

    public static HttpRequest Referrer(this HttpRequest req, string? referrer)
    {
        req.Referrer = referrer;
        return req;
    }

    public static HttpRequest TryReferrer(this HttpRequest req, string? referrer)
    {
        req.Referrer ??= referrer;
        return req;
    }

    public static HttpRequest UserAgent(this HttpRequest req, string? userAgent)
    {
        req.UserAgent = userAgent;
        return req;
    }

    public static HttpRequest TryUserAgent(this HttpRequest req, string? userAgent)
    {
        req.UserAgent ??= userAgent;
        return req;
    }

    public static string GetRequestHeader(this HttpRequest req, string? cookieHeader = null)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in req.Headers)
            sb.AppendLine($"{key}: {value}");
        if (!req.Headers.ContainsKey(HttpKnownHeaderNames.Cookie) && !cookieHeader.IsNullOrEmpty())
            sb.Append(HttpKnownHeaderNames.Cookie + ": " + cookieHeader);
        return sb.ToString();
    }

    public static string GetRequestHeader(this HttpRequest req, IEnumerable<Cookie> cookies)
    {
        return req.GetRequestHeader(cookies.Select(m => m.ToString()).JoinWith("; "));
    }

    public static string GetRequestHeader(this HttpRequest req, CookieContainer cc)
    {
        return req.GetRequestHeader(cc.GetCookies(req.GetUri()));
    }

    public static string GetRequestHeader(this HttpRequest req, IHttpService service)
    {
        return req.GetRequestHeader(service.GetCookies(req.GetUri()));
    }
}