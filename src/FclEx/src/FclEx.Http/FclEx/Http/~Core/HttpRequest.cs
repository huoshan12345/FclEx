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
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.None;
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
    public HttpContentType ReadContentType { get; set; } = HttpContentType.String;
    public bool ReadContent { get; set; } = true;

    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public UriParameterCollection Query => _uriCreator.Query;
    public UriParameterCollection Form { get; } = new(); // don't use new NameValueCollection() here.

    public string? Referrer
    {
        get => Headers.Get(HttpKnownHeaderNames.Referrer);
        set => Headers[HttpKnownHeaderNames.Referrer] = value;
    }

    public string? Origin
    {
        get => Headers.Get(HttpKnownHeaderNames.Origin);
        set => Headers[HttpKnownHeaderNames.Origin] = value;
    }

    public string? UserAgent
    {
        get => Headers.Get(HttpKnownHeaderNames.UserAgent);
        set => Headers[HttpKnownHeaderNames.UserAgent] = value;
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
        if (UserName.IsNotEmpty() && Password.IsNotEmpty())
        {
            this.BasicAuth(UserName, Password);
        }
        Method = method;
        Headers[HttpKnownHeaderNames.UserAgent] = HttpConstants.DefaultUserAgent;
    }

    public Uri GetUri() => _uriCreator.Build();

    public HttpRequest AddQueryValue(string key, string? value)
    {
        _uriCreator.AddQueryParameter(key, value);
        return this;
    }
}