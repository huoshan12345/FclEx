namespace FclEx.Http;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public partial class HttpRequest
{
    private readonly UriCreator _uriCreator;

    /// <summary>
    /// Indicates whether the final response should be validated with <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>.
    /// Redirection is handled before this check, so the status code being validated is the last response that is returned to the caller.
    /// </summary>
    public bool EnsureSuccessStatusCode { get; set; }

    /// <summary>
    /// The HTTP method used when building the outgoing <see cref="HttpRequestMessage"/>.
    /// During redirects, methods are preserved only for status codes that preserve method and content; other redirects are converted to GET, except HEAD remains HEAD.
    /// </summary>
    public HttpMethod Method { get; set; }

    /// <summary>
    /// The request body to send for non-GET requests.
    /// The content is buffered before sending so it can be replayed for retries or redirects that preserve the request body, and the original content is disposed after execution.
    /// </summary>
    public HttpContent? Content { get; set; }
#if NET6_0_OR_GREATER
    /// <summary>
    /// The HTTP version negotiation policy copied to <see cref="HttpRequestMessage.VersionPolicy"/>.
    /// </summary>
    public HttpVersionPolicy VersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;
#endif

    /// <summary>
    /// The HTTP version copied to <see cref="HttpRequestMessage.Version"/>.
    /// </summary>
    public Version Version { get; set; } = HttpVersion.Version11;

    /// <summary>
    /// Optional buffer size used when buffering request content or reading response content.
    /// A <see langword="null"/> value lets the lower-level stream helpers use their default buffer size.
    /// </summary>
    public int? BufferSize { get; set; }

    /// <summary>
    /// Optional timeout for the whole request workflow, including buffering request content, sending, redirects, response content reading, and cleanup.
    /// A <see langword="null"/> value leaves the workflow controlled by the caller's cancellation token and the underlying <see cref="HttpClient"/>.
    /// </summary>
    public TimeSpan? TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Optional timeout used while buffering request content and reading response bodies into memory.
    /// It does not limit waiting for response headers; use <see cref="ReadHeadersTimeout"/> for that.
    /// </summary>
    public TimeSpan? ReadBufferTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Optional timeout applied while waiting for response headers from each send attempt.
    /// Redirects and retries each create a fresh send attempt under the overall <see cref="TotalTimeout"/>.
    /// </summary>
    public TimeSpan? ReadHeadersTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The media type to apply to outgoing request content when the content already has a <see cref="HttpContentHeaders.ContentType"/> header without a media type.
    /// It is not used for GET requests or requests without content.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Character set to apply to outgoing request content and to prefer when decoding string responses.
    /// For responses, this value takes precedence over the response Content-Type charset.
    /// </summary>
    public string? CharSet { get; set; }

    /// <summary>
    /// Indicates whether string responses with an HTML media type should be scanned for a meta charset when no charset or BOM is available.
    /// Only the beginning of the response buffer is inspected.
    /// </summary>
    public bool DetectCharSet { get; set; }

    /// <summary>
    /// Character set to use when a string response does not specify a charset, has no detectable BOM, and optional HTML charset detection does not find one.
    /// UTF-8 is used when this value is <see langword="null"/> or invalid and invalid charsets are ignored.
    /// </summary>
    public string? FallbackCharSet { get; set; }

    /// <summary>
    /// Indicates whether invalid charset names should be ignored while decoding a string response.
    /// When <see langword="false"/>, an invalid charset from request options, response headers, or HTML meta tags is wrapped in an <see cref="InvalidOperationException"/>.
    /// </summary>
    public bool IgnoreInvalidCharSet { get; set; } = true;

    /// <summary>
    /// Compression method applied to outgoing request content for non-GET requests.
    /// Redirects that do not preserve the request body reset this value to <see cref="CompressionMethod.None"/>.
    /// </summary>
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.None;

    /// <summary>
    /// Compression level used when <see cref="CompressionMethod"/> wraps outgoing request content.
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;

    /// <summary>
    /// Determines how response content is exposed on <see cref="HttpResponse"/> when <see cref="ReadContent"/> is enabled.
    /// String and byte responses are read into memory; stream responses keep the final <see cref="HttpResponseMessage"/> alive through <see cref="HttpResponseStream"/>.
    /// </summary>
    public HttpContentType ResponseContentType { get; set; } = HttpContentType.String;

    /// <summary>
    /// Indicates whether the final response body should be read.
    /// Response headers, status code, visited URIs, and cookies can still be collected when this is disabled.
    /// </summary>
    public bool ReadContent { get; set; } = true;

    /// <summary>
    /// Indicates whether Set-Cookie headers should be parsed into the response and saved into the service cookie container.
    /// </summary>
    public bool ReadCookies { get; set; } = true;

    /// <summary>
    /// Maximum number of redirects followed by the service before throwing.
    /// Values less than or equal to zero disable following redirects.
    /// </summary>
    public int MaxRedirectCount { get; set; } = 50;

    /// <summary>
    /// Indicates whether redirects from HTTPS to HTTP are allowed.
    /// When disabled, such redirects stop the redirect loop and the downgrade response is returned to the caller.
    /// </summary>
    public bool AllowInsecureRedirects { get; set; } = true;

    /// <summary>
    /// Indicates whether a default User-Agent header should be applied
    /// when none is explicitly provided. <see cref="HttpConstants.DefaultUserAgent"/>
    /// </summary>
    public bool UseDefaultUserAgent { get; set; } = true;

    /// <summary>
    /// Indicates whether headers should be added without validating their format or allowed characters.
    /// </summary>
    public bool AddHeaderWithoutValidation { get; set; } = false;

    /// <summary>
    /// Headers to copy to the outgoing request.
    /// Content-Type, Content-Length, and Cookie are handled specially and are not copied through the normal header path.
    /// </summary>
    public HttpHeaders Headers { get; } = [];

    /// <summary>
    /// Query parameters used when rebuilding the request URI.
    /// </summary>
    public UriParams Query => _uriCreator.Query;

    /// <summary>
    /// Form values sent as <see cref="FormUrlEncodedContent"/> for non-GET requests when <see cref="Content"/> is not provided.
    /// </summary>
    public UriParams Form { get; } = []; // don't use new NameValueCollection() here.

    /// <summary>
    /// Shortcut for the Referrer request header.
    /// </summary>
    public string? Referrer
    {
        get => Headers.Get(HttpHeaderNames.Referrer);
        set => Headers.Set(HttpHeaderNames.Referrer, value);
    }

    /// <summary>
    /// Shortcut for the Authorization request header.
    /// If this is not set and <see cref="UserName"/> is provided, the service creates a Basic authorization header from <see cref="UserName"/> and <see cref="Password"/>.
    /// </summary>
    public string? Authorization
    {
        get => Headers.Get(HttpHeaderNames.Authorization);
        set => Headers.Set(HttpHeaderNames.Authorization, value);
    }

    /// <summary>
    /// Shortcut for the Origin request header.
    /// </summary>
    public string? Origin
    {
        get => Headers.Get(HttpHeaderNames.Origin);
        set => Headers.Set(HttpHeaderNames.Origin, value);
    }

    /// <summary>
    /// Shortcut for the User-Agent request header.
    /// When empty and <see cref="UseDefaultUserAgent"/> is enabled, the service applies <see cref="HttpConstants.DefaultUserAgent"/>.
    /// </summary>
    public string? UserAgent
    {
        get => Headers.Get(HttpHeaderNames.UserAgent);
        set => Headers.Set(HttpHeaderNames.UserAgent, value);
    }

    /// <summary>
    /// URI fragment used when rebuilding the request URI.
    /// </summary>
    public string Fragment
    {
        get => _uriCreator.Fragment;
        set => _uriCreator.Fragment = value;
    }

    /// <summary>
    /// URI host used when rebuilding the request URI.
    /// </summary>
    public string Host
    {
        get => _uriCreator.Host;
        set => _uriCreator.Host = value;
    }

    /// <summary>
    /// User name part of the URI user info.
    /// It is also used as Basic authentication input when no Authorization header is set.
    /// </summary>
    public string UserName
    {
        get => _uriCreator.UserName;
        set => _uriCreator.UserName = value;
    }

    /// <summary>
    /// Password part of the URI user info.
    /// It is also used as Basic authentication input when no Authorization header is set and <see cref="UserName"/> is present.
    /// </summary>
    public string Password
    {
        get => _uriCreator.Password;
        set => _uriCreator.Password = value;
    }

    /// <summary>
    /// URI path used when rebuilding the request URI.
    /// </summary>
    public string Path
    {
        get => _uriCreator.Path;
        set => _uriCreator.Path = value;
    }

    /// <summary>
    /// URI port used when rebuilding the request URI.
    /// </summary>
    public int Port
    {
        get => _uriCreator.Port;
        set => _uriCreator.Port = value;
    }

    /// <summary>
    /// URI scheme used when rebuilding the request URI.
    /// </summary>
    public string Scheme
    {
        get => _uriCreator.Scheme;
        set => _uriCreator.Scheme = value;
    }

    /// <summary>
    /// Creates a request model from a URI and HTTP method.
    /// The URI is stored through a mutable URI builder so properties such as <see cref="Query"/>, <see cref="Path"/>, and <see cref="Fragment"/> can be changed before sending.
    /// </summary>
    /// <param name="uri">Absolute or relative URI used as the initial request URI.</param>
    /// <param name="method">HTTP method used for the outgoing request.</param>
    public HttpRequest(Uri uri, HttpMethod method)
    {
        _uriCreator = new UriCreator(uri);
        Method = method;
    }

    /// <summary>
    /// Builds the current URI from the original URI and any URI-related properties or query values that were changed on this request.
    /// </summary>
    public Uri GetUri() => _uriCreator.Build();
}
