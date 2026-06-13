namespace FclEx.Http;

partial class HttpRequestExtensions
{
    /// <summary>
    /// Enables or disables reading the final response body.
    /// </summary>
    public static HttpRequest ReadContent(this HttpRequest request, bool value)
    {
        request.ReadContent = value;
        return request;
    }

    /// <summary>
    /// Sets the maximum number of redirects followed by the service.
    /// </summary>
    public static HttpRequest MaxRedirectCount(this HttpRequest request, int value)
    {
        request.MaxRedirectCount = value;
        return request;
    }

    /// <summary>
    /// Controls whether HTTPS-to-HTTP redirects may be followed.
    /// </summary>
    public static HttpRequest AllowInsecureRedirects(this HttpRequest request, bool value = true)
    {
        request.AllowInsecureRedirects = value;
        return request;
    }

    /// <summary>
    /// Enables or disables parsing and saving response cookies.
    /// </summary>
    public static HttpRequest ReadCookies(this HttpRequest request, bool value)
    {
        request.ReadCookies = value;
        return request;
    }

    /// <summary>
    /// Controls whether the default User-Agent is applied when no User-Agent is set.
    /// </summary>
    public static HttpRequest UseDefaultUserAgent(this HttpRequest request, bool value)
    {
        request.UseDefaultUserAgent = value;
        return request;
    }

    /// <summary>
    /// Controls whether request headers are added with format validation disabled.
    /// </summary>
    public static HttpRequest AddHeaderWithoutValidation(this HttpRequest request, bool value)
    {
        request.AddHeaderWithoutValidation = value;
        return request;
    }

    /// <summary>
    /// Controls whether the final response status code is validated as successful.
    /// </summary>
    public static HttpRequest EnsureSuccessStatusCode(this HttpRequest request, bool value = true)
    {
        request.EnsureSuccessStatusCode = value;
        return request;
    }

    /// <summary>
    /// Sets the host used when rebuilding the request URI.
    /// </summary>
    public static HttpRequest Host(this HttpRequest request, string host)
    {
        request.Host = host;
        return request;
    }

    /// <summary>
    /// Sets the port used when rebuilding the request URI.
    /// </summary>
    public static HttpRequest Port(this HttpRequest request, int port)
    {
        request.Port = port;
        return request;
    }

    /// <summary>
    /// Sets the fragment used when rebuilding the request URI.
    /// </summary>
    public static HttpRequest Fragment(this HttpRequest request, string fragment)
    {
        request.Fragment = fragment;
        return request;
    }

    /// <summary>
    /// Sets the URI user name and potential Basic authentication user name.
    /// </summary>
    public static HttpRequest UserName(this HttpRequest request, string userName)
    {
        request.UserName = userName;
        return request;
    }

    /// <summary>
    /// Sets the URI password and potential Basic authentication password.
    /// </summary>
    public static HttpRequest Password(this HttpRequest request, string password)
    {
        request.Password = password;
        return request;
    }

    /// <summary>
    /// Sets the path used when rebuilding the request URI.
    /// </summary>
    public static HttpRequest Path(this HttpRequest request, string path)
    {
        request.Path = path;
        return request;
    }

    /// <summary>
    /// Sets the scheme used when rebuilding the request URI.
    /// </summary>
    public static HttpRequest Scheme(this HttpRequest request, string scheme)
    {
        request.Scheme = scheme;
        return request;
    }

    /// <summary>
    /// Sets the HTTP method used when sending the request.
    /// </summary>
    public static HttpRequest Method(this HttpRequest request, HttpMethod method)
    {
        request.Method = method;
        return request;
    }

    /// <summary>
    /// Sets the HTTP method from a method name.
    /// </summary>
    public static HttpRequest Method(this HttpRequest request, string method)
    {
        return request.Method(new HttpMethod(method));
    }

    /// <summary>
    /// Sets the Authorization header to an already formatted value.
    /// </summary>
    public static HttpRequest Auth(this HttpRequest request, string? auth)
    {
        return request.SetHeader(HttpHeaderNames.Authorization, auth);
    }

    /// <summary>
    /// Sets a Basic authorization header from a user name and password.
    /// </summary>
    public static HttpRequest BasicAuth(this HttpRequest request, string? userName, string? password)
    {
        var userInfo = userName + ":" + password;
        return request.SetHeader(HttpHeaderNames.Authorization, "Basic " + userInfo.ToBytes().ToBase64());
    }

    /// <summary>
    /// Sets a Bearer authorization header from a token.
    /// </summary>
    public static HttpRequest BearerAuth(this HttpRequest request, string token)
    {
        return request.SetHeader(HttpHeaderNames.Authorization, "Bearer " + token);
    }

    /// <summary>
    /// Sets the preferred charset for request content and string response decoding.
    /// </summary>
    public static HttpRequest CharSet(this HttpRequest request, string? charSet)
    {
        request.CharSet = charSet;
        return request;
    }

    /// <summary>
    /// Sets the preferred charset only when it is currently unset.
    /// </summary>
    public static HttpRequest TryCharSet(this HttpRequest request, string? charSet)
    {
        request.CharSet ??= charSet;
        return request;
    }

    /// <summary>
    /// Controls whether HTML responses are scanned for a meta charset when no charset is otherwise available.
    /// </summary>
    public static HttpRequest DetectCharSet(this HttpRequest request, bool flag = true)
    {
        request.DetectCharSet = flag;
        return request;
    }

    /// <summary>
    /// Sets the charset used when response decoding cannot determine one.
    /// </summary>
    public static HttpRequest FallbackCharSet(this HttpRequest request, string? charSet)
    {
        request.FallbackCharSet = charSet;
        return request;
    }

    /// <summary>
    /// Sets the fallback charset only when it is currently unset.
    /// </summary>
    public static HttpRequest TryFallbackCharSet(this HttpRequest request, string? charSet)
    {
        request.FallbackCharSet ??= charSet;
        return request;
    }

    /// <summary>
    /// Sets the timeout for waiting for response headers on each send attempt.
    /// </summary>
    public static HttpRequest ReadHeadersTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadHeadersTimeout = timeout;
        return request;
    }

    /// <summary>
    /// Sets the header-read timeout only when it is currently unset.
    /// </summary>
    public static HttpRequest TryReadHeadersTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadHeadersTimeout ??= timeout;
        return request;
    }

    /// <summary>
    /// Sets the timeout for buffering request content or reading response bodies into memory.
    /// </summary>
    public static HttpRequest ReadBufferTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadBufferTimeout = timeout;
        return request;
    }

    /// <summary>
    /// Sets the buffer-read timeout only when it is currently unset.
    /// </summary>
    public static HttpRequest TryReadBufferTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.ReadBufferTimeout ??= timeout;
        return request;
    }

    /// <summary>
    /// Sets the buffer size used by package stream-copy helpers.
    /// </summary>
    public static HttpRequest BufferSize(this HttpRequest request, int? bufferSize)
    {
        request.BufferSize = bufferSize;
        return request;
    }

    /// <summary>
    /// Sets the buffer size only when it is currently unset.
    /// </summary>
    public static HttpRequest TryBufferSize(this HttpRequest request, int? bufferSize)
    {
        request.BufferSize ??= bufferSize;
        return request;
    }

    /// <summary>
    /// Sets the timeout for the whole request workflow.
    /// </summary>
    public static HttpRequest TotalTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.TotalTimeout = timeout;
        return request;
    }

    /// <summary>
    /// Sets the total timeout only when it is currently unset.
    /// </summary>
    public static HttpRequest TryTotalTimeout(this HttpRequest request, TimeSpan? timeout)
    {
        request.TotalTimeout ??= timeout;
        return request;
    }

    /// <summary>
    /// Sets the Origin header shortcut.
    /// </summary>
    public static HttpRequest Origin(this HttpRequest request, string? url)
    {
        request.Origin = url;
        return request;
    }

    /// <summary>
    /// Sets the Origin header shortcut only when it is currently unset.
    /// </summary>
    public static HttpRequest TryOrigin(this HttpRequest request, string? url)
    {
        request.Origin ??= url;
        return request;
    }

    /// <summary>
    /// Sets how response content should be exposed on <see cref="HttpResponse"/>.
    /// </summary>
    public static HttpRequest ReadAs(this HttpRequest request, HttpContentType value)
    {
        request.ResponseContentType = value;
        return request;
    }

    /// <summary>
    /// Configures the response body to be read as a string.
    /// </summary>
    public static HttpRequest ReadAsString(this HttpRequest request) => request.ReadAs(HttpContentType.String);

    /// <summary>
    /// Configures the response body to be read as bytes.
    /// </summary>
    public static HttpRequest ReadAsBytes(this HttpRequest request) => request.ReadAs(HttpContentType.Bytes);

    /// <summary>
    /// Configures the response body to be exposed as a stream.
    /// </summary>
    public static HttpRequest ReadAsStream(this HttpRequest request) => request.ReadAs(HttpContentType.Stream);

    /// <summary>
    /// Sets the HTTP version copied to the outgoing request message.
    /// </summary>
    public static HttpRequest Version(this HttpRequest request, Version version)
    {
        request.Version = version;
        return request;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Sets the HTTP version negotiation policy copied to the outgoing request message.
    /// </summary>
    public static HttpRequest VersionPolicy(this HttpRequest request, HttpVersionPolicy policy)
    {
        request.VersionPolicy = policy;
        return request;
    }
#endif
}
