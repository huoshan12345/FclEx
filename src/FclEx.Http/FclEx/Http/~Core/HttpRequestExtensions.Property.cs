namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest ReadContent(this HttpRequest request, bool value)
    {
        request.ReadContent = value;
        return request;
    }

    public static HttpRequest ReadCookies(this HttpRequest request, bool value)
    {
        request.ReadCookies = value;
        return request;
    }

    public static HttpRequest UseDefaultUserAgent(this HttpRequest request, bool value)
    {
        request.UseDefaultUserAgent = value;
        return request;
    }

    public static HttpRequest AddHeaderWithoutValidation(this HttpRequest request, bool value)
    {
        request.AddHeaderWithoutValidation = value;
        return request;
    }

    public static HttpRequest EnsureSuccessStatusCode(this HttpRequest request, bool value = true)
    {
        request.EnsureSuccessStatusCode = value;
        return request;
    }

    public static HttpRequest Host(this HttpRequest request, string host)
    {
        request.Host = host;
        return request;
    }

    public static HttpRequest Port(this HttpRequest request, int port)
    {
        request.Port = port;
        return request;
    }

    public static HttpRequest Fragment(this HttpRequest request, string fragment)
    {
        request.Fragment = fragment;
        return request;
    }

    public static HttpRequest UserName(this HttpRequest request, string userName)
    {
        request.UserName = userName;
        return request;
    }

    public static HttpRequest Password(this HttpRequest request, string password)
    {
        request.Password = password;
        return request;
    }

    public static HttpRequest Path(this HttpRequest request, string path)
    {
        request.Path = path;
        return request;
    }

    public static HttpRequest Scheme(this HttpRequest request, string scheme)
    {
        request.Scheme = scheme;
        return request;
    }

    public static HttpRequest Method(this HttpRequest request, HttpMethod method)
    {
        request.Method = method;
        return request;
    }

    public static HttpRequest Method(this HttpRequest request, string method)
    {
        return request.Method(new HttpMethod(method));
    }

    public static HttpRequest Auth(this HttpRequest request, string? auth)
    {
        return request.SetHeader(HttpHeaderNames.Authorization, auth);
    }

    public static HttpRequest BasicAuth(this HttpRequest request, string? userName, string? password)
    {
        var userInfo = userName + ":" + password;
        return request.SetHeader(HttpHeaderNames.Authorization, "Basic " + userInfo.ToBytes().ToBase64());
    }

    public static HttpRequest BearerAuth(this HttpRequest request, string token)
    {
        return request.SetHeader(HttpHeaderNames.Authorization, "Bearer " + token);
    }

    public static HttpRequest CharSet(this HttpRequest request, string? chartSet)
    {
        request.CharSet = chartSet;
        return request;
    }

    public static HttpRequest TryCharSet(this HttpRequest request, string? chartSet)
    {
        request.CharSet = chartSet;
        return request;
    }

    public static HttpRequest DetectCharSet(this HttpRequest request, bool flag = true)
    {
        request.DetectCharSet = flag;
        return request;
    }

    public static HttpRequest FallbackCharSet(this HttpRequest request, string? chartSet)
    {
        request.FallbackCharSet = chartSet;
        return request;
    }

    public static HttpRequest TryFallbackCharSet(this HttpRequest request, string? chartSet)
    {
        request.FallbackCharSet ??= chartSet;
        return request;
    }

    public static HttpRequest ReadHeadersTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadHeadersTimeout = timeout;
        return request;
    }

    public static HttpRequest TryReadHeadersTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadHeadersTimeout ??= timeout;
        return request;
    }

    public static HttpRequest ReadBufferTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadBufferTimeout = timeout;
        return request;
    }

    public static HttpRequest TryReadBufferTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadBufferTimeout ??= timeout;
        return request;
    }

    public static HttpRequest TotalTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.TotalTimeout = timeout;
        return request;
    }

    public static HttpRequest TryTotalTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.TotalTimeout ??= timeout;
        return request;
    }

    public static HttpRequest Origin(this HttpRequest request, string? url)
    {
        request.Origin = url;
        return request;
    }

    public static HttpRequest TryOrigin(this HttpRequest request, string? url)
    {
        request.Origin ??= url;
        return request;
    }

    public static HttpRequest ReadAs(this HttpRequest request, HttpContentType value)
    {
        request.ResponseContentType = value;
        return request;
    }

    public static HttpRequest ReadAsString(this HttpRequest request) => request.ReadAs(HttpContentType.String);

    public static HttpRequest ReadAsBytes(this HttpRequest request) => request.ReadAs(HttpContentType.Bytes);

    public static HttpRequest ReadAsStream(this HttpRequest request) => request.ReadAs(HttpContentType.Stream);

    public static HttpRequest Version(this HttpRequest request, Version version)
    {
        request.Version = version;
        return request;
    }

#if NET6_0_OR_GREATER
    public static HttpRequest VersionPolicy(this HttpRequest request, HttpVersionPolicy policy)
    {
        request.VersionPolicy = policy;
        return request;
    }
#endif
}