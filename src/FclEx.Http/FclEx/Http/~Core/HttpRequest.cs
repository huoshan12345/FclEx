namespace FclEx.Http;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public partial class HttpRequest
{
    private readonly UriCreator _uriCreator;

    public bool EnsureSuccessStatusCode { get; set; }
    public HttpMethod Method { get; set; }
    public HttpContent? Content { get; set; }
#if NET6_0_OR_GREATER
    public HttpVersionPolicy VersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;
#endif
    public Version Version { get; set; } = HttpVersion.Version11;
    public int BufferSize { get; set; } = 256 * 1024;
    public TimeSpan? TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan? ReadBufferTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan? ReadHeadersTimeout { get; set; } = TimeSpan.FromSeconds(10);
    /// <summary>
    /// Gets or sets the value that will be used as <see cref="ContentType.MediaType"/>
    /// </summary>
    public string? MediaType { get; set; }
    /// <summary>
    /// Gets or sets the value that will be used as <see cref="ContentType.CharSet"/>
    /// </summary>
    public string? CharSet { get; set; }
    public bool DetectCharSet { get; set; }
    public string? FallbackCharSet { get; set; }
    public bool IgnoreInvalidCharSet { get; set; } = true;
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.None;
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;
    public HttpContentType ResponseContentType { get; set; } = HttpContentType.String;
    public bool ReadContent { get; set; } = true;
    public bool ReadCookies { get; set; } = true;
    public int MaxRedirectCount { get; set; } = 50;

    /// <summary>
    /// Indicates whether a default User-Agent header should be applied
    /// when none is explicitly provided. <see cref="HttpConstants.DefaultUserAgent"/>
    /// </summary>
    public bool UseDefaultUserAgent { get; set; } = true;

    /// <summary>
    /// Indicates whether headers should be added without validating their format or allowed characters.
    /// </summary>
    public bool AddHeaderWithoutValidation { get; set; } = false;

    public HttpHeaders Headers { get; } = [];
    public UriParams Query => _uriCreator.Query;
    public UriParams Form { get; } = []; // don't use new NameValueCollection() here.

    public string? Referrer
    {
        get => Headers.Get(HttpHeaderNames.Referrer);
        set => Headers.Set(HttpHeaderNames.Referrer, value);
    }

    public string? Authorization
    {
        get => Headers.Get(HttpHeaderNames.Authorization);
        set => Headers.Set(HttpHeaderNames.Authorization, value);
    }

    public string? Origin
    {
        get => Headers.Get(HttpHeaderNames.Origin);
        set => Headers.Set(HttpHeaderNames.Origin, value);
    }

    public string? UserAgent
    {
        get => Headers.Get(HttpHeaderNames.UserAgent);
        set => Headers.Set(HttpHeaderNames.UserAgent, value);
    }

    public string Fragment
    {
        get => _uriCreator.Fragment;
        set => _uriCreator.Fragment = value;
    }
    public string Host
    {
        get => _uriCreator.Host;
        set => _uriCreator.Host = value;
    }
    public string UserName
    {
        get => _uriCreator.UserName;
        set => _uriCreator.UserName = value;
    }
    public string Password
    {
        get => _uriCreator.Password;
        set => _uriCreator.Password = value;
    }
    public string Path
    {
        get => _uriCreator.Path;
        set => _uriCreator.Path = value;
    }
    public int Port
    {
        get => _uriCreator.Port;
        set => _uriCreator.Port = value;
    }
    public string Scheme
    {
        get => _uriCreator.Scheme;
        set => _uriCreator.Scheme = value;
    }

    public HttpRequest(Uri uri, HttpMethod method)
    {
        _uriCreator = new UriCreator(uri);
        Method = method;
    }

    public Uri GetUri() => _uriCreator.Build();
}
