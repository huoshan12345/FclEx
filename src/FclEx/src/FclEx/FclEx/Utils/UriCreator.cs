using System.Collections.Specialized;
using System.Web;
#pragma warning disable IDE0057

namespace FclEx.Utils;

public partial class UriCreator
{
    public UriCreator(string uri)
        : this(new Uri(uri, UriKind.RelativeOrAbsolute))
    {
    }

    public UriCreator(Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            _uriBuilder = new UriBuilder(uri);
        }
        else
        {
            var str = uri.ToString();
            var (path, query, fragment) = SplitUri(str);
            _uriBuilder = new UriBuilder(Uri.UriSchemeHttp, "", 80, path)
            {
                Query = query,
                Fragment = fragment
            };
        }

        QueryValues = HttpUtility.ParseQueryString(_uriBuilder.Query);
        _uriBuilder.Query = string.Empty;
    }

    private readonly UriBuilder _uriBuilder;

    public NameValueCollection QueryValues { get; }

    public string Fragment
    {
        get => _uriBuilder.Fragment;
        set => _uriBuilder.Fragment = value;
    }

    [AllowNull]
    public string Host
    {
        get => _uriBuilder.Host;
        set
        {
            if (value.IsNullOrEmpty())
            {
                _uriBuilder.Host = "";
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
                    _uriBuilder.Host = h;
                    _uriBuilder.Port = p;
                }
            }
            else
            {
                _uriBuilder.Host = value;
            }
        }
    }
    public string UserName
    {
        get => _uriBuilder.UserName;
        set => _uriBuilder.UserName = value;
    }
    public string Password
    {
        get => _uriBuilder.Password;
        set => _uriBuilder.Password = value;
    }
    public string Path
    {
        get => _uriBuilder.Path;
        set => _uriBuilder.Path = value;
    }
    public int Port
    {
        get => _uriBuilder.Port;
        set => _uriBuilder.Port = value;
    }
    public string Scheme
    {
        get => _uriBuilder.Scheme;
        set => _uriBuilder.Scheme = value;
    }

    public Uri GetUri()
    {
        if (Host.IsValid())
        {
            if (QueryValues.Count == 0)
                return _uriBuilder.Uri;

            _uriBuilder.Query = QueryValues.ToString();
            var uri = _uriBuilder.Uri;
            _uriBuilder.Query = string.Empty;
            return uri;
        }
        else
        {
            var u = _uriBuilder.Path;
            var (q, f) = (QueryValues.ToString(), Fragment);
            if (q.IsValid())
            {
                u = u + "?" + q;
            }
            if (f.IsValid())
            {
                u += f; // "f" contains a '#'
            }
            return new Uri(u, UriKind.Relative);
        }

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