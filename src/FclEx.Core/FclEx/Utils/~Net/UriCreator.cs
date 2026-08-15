namespace FclEx.Utils;

/// <summary>
/// The UriCreator class provides a fluent interface for constructing URIs.<br/>
/// It allows users to specify various components of a URI, including the scheme, host, port, path, query parameters, and fragment.<br/>
/// The class ensures that the generated URI is well-formed and adheres to the URI standards.
/// </summary>
public class UriCreator
{
    private static Regex Ipv4HostPort { get; } = new(@"^([0-9]{3}\.[0-9]{3}\.[0-9]{3}\.[0-9]{3})(?::(\d+))?$", RegexOptions.Compiled);
    private static Regex Ipv6HostPort { get; } = new(@"^(\[[^\[^\]]+\])(?::(\d+))?$", RegexOptions.Compiled);
    private static Regex HostPort { get; } = new(@"^([-\w\.]+):(\d+)$", RegexOptions.Compiled);

    public UriCreator(string? scheme = null, string? host = null, int port = -1, string? path = null)
        : this(new UriBuilder(scheme ?? "", host ?? "", port, path ?? ""))
    {
    }

    public UriCreator(UriBuilder builder)
    {
        _builder = builder;
        Query = UriParams.Parse(builder.Query);
        _builder.Query = string.Empty;
    }

    public UriCreator(string uri)
        : this(new Uri(uri, UriKind.RelativeOrAbsolute))
    {
    }

    public UriCreator(Uri uri)
    {
        Check.NotNull(uri);

        if (uri.IsAbsoluteUri)
        {
            _builder = new UriBuilder(uri);
        }
        else
        {
            // host can be set later.
            var str = uri.ToString();
            var (path, query, fragment) = SplitUri(str);
            _builder = new UriBuilder(Uri.UriSchemeHttp, "", 80, path)
            {
                Query = query,
                Fragment = fragment,
            };
        }

        Query = UriParams.Parse(_builder.Query);
        _builder.Query = string.Empty;
    }

    private readonly UriBuilder _builder;

    public string Scheme
    {
        get => _builder.Scheme;
        set => _builder.Scheme = value;
    }
    [AllowNull]
    public string Host
    {
        get => _builder.Host;
        set
        {
            if (value.IsNullOrEmpty())
            {
                _builder.Host = "";
                return;
            }

            if (Ipv6HostPort.TryMatch(value, out var match) // [ipv6]:port or [ipv6]
                || Ipv4HostPort.TryMatch(value, out match)) // ipv4:port or ipv4
            {
                var h = match.Groups[1].Value;
                var p = match.GetInt(2, -1);

                SetHost(h);
                SetPort(p);
                return;
            }

            // IPAddress.TryParse("[::1]:5"); is valid, but the :5 is silently dropped!
            // So put it between checking for IpHostPort and HostPort
            if (IPAddress.TryParse(value, out var ip))
            {
                SetHost(ip.ToString());
                return;
            }

            if (HostPort.TryMatch(value, out match)) // host:port
            {
                var h = match.Groups[1].Value;
                var p = match.GetInt(2, -1);

                SetHost(h);
                SetPort(p);
                return;
            }

            SetHost(value);
        }
    }

    private void SetHost(string value)
    {
        if (value == Host)
            return;

        _builder.Host = value;
    }

    private void SetPort(int value)
    {
        if (value == Port)
            return;

        _builder.Port = value;
    }

    public int Port
    {
        get => _builder.Port;
        set => _builder.Port = value;
    }
    public string UserName
    {
        get => _builder.UserName;
        set => _builder.UserName = value;
    }
    public string Password
    {
        get => _builder.Password;
        set => _builder.Password = value;
    }
    public string Path
    {
        get => _builder.Path;
        set => _builder.Path = value;
    }
    public UriParams Query { get; }
    public string Fragment
    {
        get => _builder.Fragment;
        // nfx will automatically add a '#' if the fragment is not empty, so we need to trim it to avoid duplication.
        set => _builder.Fragment = value.TrimStart('#');
    }

    public Uri Build()
    {
        if (Host.IsNotEmpty())
        {
            if (Query.IsEmpty())
                return _builder.Uri;

            _builder.Query = Query.ToString();
            var uri = _builder.Uri;
            _builder.Query = string.Empty;
            return uri;
        }

        var str = StringBuilderHelper.Build(m =>
        {
            m.Append(Path);
            if (Query.IsNotEmpty())
            {
                m.Append('?');
                Query.Render(m);
            }
            if (Fragment is { Length: > 0 } fragment)
            {
                m.Append(fragment); // "fragment" already contains '#'
            }
        });
        return new Uri(str, UriKind.Relative);
    }

    public override string ToString()
    {
        return Build().ToString();
    }

    [SuppressMessage("ReSharper", "ReplaceSubstringWithRangeIndexer")]
    public static (string Path, string Query, string Fragment) SplitUri(string uri)
    {
        var idx1 = uri.IndexOf('?');
        var idx2 = uri.IndexOf('#');
        return (idx1, idx2) switch
        {
            (-1, -1) => (uri, "", ""),
            (_, -1) => (uri[..idx1], uri[(idx1 + 1)..], ""),
            (-1, _) => (uri[..idx2], "", uri[(idx2 + 1)..]),
            (_, _) when idx1 < idx2 => (uri[..idx1], uri[(idx1 + 1)..idx2], uri[(idx2 + 1)..]),
            _ => (uri[..idx2], uri[(idx2 + 1)..idx1], uri[(idx1 + 1)..]), // '?' after '#'
        };
    }
}