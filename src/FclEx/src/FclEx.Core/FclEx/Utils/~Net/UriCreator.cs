namespace FclEx.Utils;

/// <summary>
/// The UriCreator class provides a fluent interface for constructing URIs.<br/>
/// It allows users to specify various components of a URI, including the scheme, host, port, path, query parameters, and fragment.<br/>
/// The class ensures that the generated URI is well-formed and adheres to the URI standards.
/// </summary>
public class UriCreator
{
    public UriCreator(string? scheme, string? host, int port = -1, string? path = null)
        : this(new UriBuilder(scheme, host, port, path))
    {
    }

    public UriCreator(UriBuilder builder)
    {
        _builder = builder;
        Query = new(_builder.Query);
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

        Query = new(_builder.Query);
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

            if (CommonRegex.Scheme.TryMatch(value, out var m))
            {
                Scheme = m.Groups[1].Value;
                value = value.TrimStart(m.Value);
            }

            var match = CommonRegex.HostPort.Match(value);
            if (!match.Success)
            {
                match = CommonRegex.Ipv6HostPort.Match(value);
            }
            if (match.Success)
            {
                var h = match.Groups[1].Value;
                var p = match.GetInt(2, 80);
                if (h != Host || p != Port)
                {
                    _builder.Host = h;
                    _builder.Port = p;
                }
            }
            else
            {
                _builder.Host = value;
            }
        }
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
        set => _builder.Fragment = value;
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

    [SuppressMessage("ReSharper", "ReplaceSubstringWithRangeIndexer")]
    public static (string Path, string Query, string Fragment) SplitUri(string uri)
    {
        var idx1 = uri.IndexOf('?');
        var idx2 = uri.IndexOf('#');
        return (idx1, idx2) switch
        {
            (-1, -1) => (uri, "", ""),
            (_, -1) => (uri.Substring(0, idx1), uri.Substring(idx1 + 1), ""),
            (-1, _) => (uri.Substring(0, idx2), "", uri.Substring(idx2 + 1)),
            (_, _) when idx1 >= idx2 => throw new ArgumentException("In URIs with a query and a fragment, the fragment should follows the query"),
            _ => (uri.Substring(0, idx1), uri.Substring(idx1 + 1, idx2 - idx1 - 1), uri.Substring(idx2 + 1)),
        };
    }
}

public static class UriCreatorExtensions
{
    public static UriCreator Scheme(this UriCreator creator, string scheme)
    {
        creator.Scheme = scheme;
        return creator;
    }

    public static UriCreator Host(this UriCreator creator, string host)
    {
        creator.Host = host;
        return creator;
    }

    public static UriCreator Port(this UriCreator creator, int port)
    {
        creator.Port = port;
        return creator;
    }

    public static UriCreator UserName(this UriCreator creator, string userName)
    {
        creator.UserName = userName;
        return creator;
    }

    public static UriCreator Path(this UriCreator creator, string path)
    {
        creator.Path = path;
        return creator;
    }

    public static UriCreator Fragment(this UriCreator creator, string fragment)
    {
        creator.Fragment = fragment;
        return creator;
    }

    public static UriCreator AddQueryParam(this UriCreator creator, string key, string? value)
    {
        creator.Query.Add(key, value);
        return creator;
    }
}