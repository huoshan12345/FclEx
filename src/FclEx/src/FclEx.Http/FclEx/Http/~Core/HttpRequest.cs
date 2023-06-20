using System.Web;

namespace FclEx.Http;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public partial class HttpRequest
{
    private readonly UriCreator _uriCreator;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public bool ThrowIfFailed { get; set; } = true;
    public HttpMethod Method { get; set; }
    public HttpContent? Content { get; set; }
    public int BufferSize { get; set; } = 256 * 1024;
    public TimeSpan? TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public TimeSpan? ReadBufferTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan? ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public bool DetectCharSet { get; set; }
    public string? FallbackCharSet { get; set; }
    public HttpResponseType ResponseType { get; set; }
    public bool ReadCookie { get; set; } = true;
    public bool ReadHeader { get; set; } = true;
    public bool ReadContent { get; set; } = true;
    public bool UseGZip { get; set; } = false;

    public NameValueCollection QueryValues => _uriCreator.QueryValues;
    public Dictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public NameValueCollection FormValues { get; } = HttpUtility.ParseQueryString(""); // don't use new NameValueCollection() here.

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

        Method = method;

        AddHeader(HttpKnownHeaderNames.UserAgent, HttpConstants.DefaultUserAgent);

        if (UserName.IsValid() && Password.IsValid())
        {
            this.BasicAuth(UserName, Password);
        }
    }

    public Uri GetUri() => _uriCreator.GetUri();

    public HttpRequest AddQueryValue(string key, string? value)
    {
        Check.NotNull(key);
        QueryValues.Add(key.Trim(), value.ToStringOrEmpty().Trim());
        return this;
    }

    public HttpRequest AddFormValue(string key, string? value)
    {
        Check.NotNull(key);
        FormValues.Add(key.Trim(), value.ToStringOrEmpty().Trim());
        return this;
    }

    public HttpRequest AddHeader(string key, string? value)
    {
        Check.NotNull(key);
        Headers[key.Trim()] = value.ToStringOrEmpty().Trim();
        return this;
    }
}