namespace FclEx.Http;

partial class HttpRequestExtensions
{
    public static HttpRequest ReadContent(this HttpRequest req, bool read)
    {
        req.ReadContent = read;
        return req;
    }

    public static HttpRequest EnsureSuccessStatusCode(this HttpRequest req, bool value)
    {
        req.EnsureSuccessStatusCode = value;
        return req;
    }

    public static HttpRequest Host(this HttpRequest req, string host)
    {
        req.Host = host;
        return req;
    }

    public static HttpRequest Port(this HttpRequest req, int port)
    {
        req.Port = port;
        return req;
    }

    public static HttpRequest Fragment(this HttpRequest req, string fragment)
    {
        req.Fragment = fragment;
        return req;
    }

    public static HttpRequest UserName(this HttpRequest req, string userName)
    {
        req.UserName = userName;
        return req;
    }

    public static HttpRequest Password(this HttpRequest req, string password)
    {
        req.Password = password;
        return req;
    }

    public static HttpRequest Path(this HttpRequest req, string path)
    {
        req.Path = path;
        return req;
    }

    public static HttpRequest Scheme(this HttpRequest req, string scheme)
    {
        req.Scheme = scheme;
        return req;
    }

    public static HttpRequest Method(this HttpRequest req, HttpMethod method)
    {
        req.Method = method;
        return req;
    }

    public static HttpRequest Method(this HttpRequest req, string method)
    {
        return req.Method(new HttpMethod(method));
    }

    public static HttpRequest Auth(this HttpRequest req, string? auth)
    {
        return req.AddHeader(HttpKnownHeaderNames.Authorization, auth);
    }

    public static HttpRequest BasicAuth(this HttpRequest req, string? userName, string? password)
    {
        var userInfo = userName + ":" + password;
        return req.AddHeader(HttpKnownHeaderNames.Authorization, "Basic " + userInfo.ToBytes().ToBase64());
    }

    public static HttpRequest BearerAuth(this HttpRequest req, string token)
    {
        return req.AddHeader(HttpKnownHeaderNames.Authorization, "Bearer " + token);
    }

    public static HttpRequest CharSet(this HttpRequest req, string? chartSet)
    {
        req.CharSet = chartSet;
        return req;
    }

    public static HttpRequest TryCharSet(this HttpRequest req, string? chartSet)
    {
        req.CharSet = chartSet;
        return req;
    }

    public static HttpRequest DetectCharSet(this HttpRequest req, bool flag = true)
    {
        req.DetectCharSet = flag;
        return req;
    }

    public static HttpRequest FallbackCharSet(this HttpRequest req, string? chartSet)
    {
        req.FallbackCharSet = chartSet;
        return req;
    }

    public static HttpRequest TryFallbackCharSet(this HttpRequest req, string? chartSet)
    {
        req.FallbackCharSet ??= chartSet;
        return req;
    }

    public static HttpRequest ReadHeadersTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.ReadHeadersTimeout = timeout;
        return req;
    }

    public static HttpRequest TryReadHeadersTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.ReadHeadersTimeout ??= timeout;
        return req;
    }
    
    public static HttpRequest ReadBufferTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.ReadBufferTimeout = timeout;
        return req;
    }

    public static HttpRequest TryReadBufferTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.ReadBufferTimeout ??= timeout;
        return req;
    }

    public static HttpRequest TotalTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.TotalTimeout = timeout;
        return req;
    }

    public static HttpRequest TryTotalTimeout(this HttpRequest req, TimeSpan? timeout)
    {
        req.TotalTimeout ??= timeout;
        return req;
    }

    public static HttpRequest Origin(this HttpRequest req, string? url)
    {
        req.Origin = url;
        return req;
    }

    public static HttpRequest TryOrigin(this HttpRequest req, string? url)
    {
        req.Origin ??= url;
        return req;
    }

    public static HttpRequest ReadAs(this HttpRequest req, HttpContentType type)
    {
        req.ReadContentType = type;
        return req;
    }

    public static HttpRequest ReadAsString(this HttpRequest req) => req.ReadAs(HttpContentType.String);

    public static HttpRequest ReadAsBytes(this HttpRequest req) => req.ReadAs(HttpContentType.Bytes);

    public static HttpRequest ReadAsStream(this HttpRequest req) => req.ReadAs(HttpContentType.Stream);
    
    public static HttpRequest Version(this HttpRequest req, Version version)
    {
        req.Version = version;
        return req;
    }

    public static HttpRequest VersionPolicy(this HttpRequest req, HttpVersionPolicy policy)
    {
        req.VersionPolicy = policy;
        return req;
    }
}