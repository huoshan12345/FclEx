using System;
using System.Net;
using FclEx.Extensions;

namespace FclEx.Http;

public sealed class WebProxyExt : IWebProxyExt, IEquatable<WebProxyExt>
{
    public ProxyType Type { get; }
    public string? Host { get; }
    public int Port { get; }
    private readonly Uri? _uri;
    private NetworkCredential? _credentials;

    public WebProxyExt(ProxyType type, string? host, int port, NetworkCredential? credentials = null)
    {
        Type = type;
        Host = host ?? "localhost";
        Port = port;
        _credentials = credentials;
        if (Type == ProxyType.Http)
            _uri = new UriBuilder(Uri.UriSchemeHttp, Host, port).Uri;
        else if (Type == ProxyType.Https)
            _uri = new UriBuilder(Uri.UriSchemeHttp, Host, port).Uri;
    }

    private WebProxyExt(Uri uri, NetworkCredential? credentials)
    {
        Check.NotNull(uri);
        if (uri.Scheme == Uri.UriSchemeHttp)
            Type = ProxyType.Http;
        else if (uri.Scheme == Uri.UriSchemeHttps)
            Type = ProxyType.Https;
        else throw new ArgumentException(uri.Scheme);

        _uri = uri;
        Host = uri.Host;
        Port = uri.Port;
        if (credentials == null)
        {
            var userInfo = uri.UserInfo.Split(':');
            if (userInfo.Length == 2)
            {
                _credentials = new NetworkCredential(userInfo[0].UrlDecode(), userInfo[1].UrlDecode());
            }
        }
        else
        {
            _credentials = credentials;
        }
    }

    public static WebProxyExt Create(Uri? uri, NetworkCredential? credentials = null)
    {
        return uri == null ? None : new WebProxyExt(uri, credentials);
    }

    public static WebProxyExt Create(string? url, NetworkCredential? credentials = null)
    {
        var uri = url.IsValid() ? new Uri(url) : null;
        return Create(uri, credentials);
    }

    public static WebProxyExt None { get; set; } = new(ProxyType.None, null, 0);

    public Uri? GetProxy(Uri destination) => _uri;

    public bool IsBypassed(Uri uri) => uri.IsLoopback;

    public ICredentials? Credentials
    {
        get => _credentials;
        set
        {
            if (Type == ProxyType.None) throw new NotSupportedException();
            if (value != null)
            {
                _credentials = value as NetworkCredential;
                Check.NotNull(_credentials!);
            }
            else
            {
                _credentials = null;
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is WebProxyExt o && Equals(o);
    }

    public bool Equals(WebProxyExt? other)
    {
        return !(other is null)
               && _uri == other._uri
               && Type == other.Type
               && Host == other.Host
               && Port == other.Port
               && Credentials == other.Credentials;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (_uri != null ? _uri.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int)Type;
            hashCode = (hashCode * 397) ^ (Host != null ? Host.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Port;
            // hashCode = (hashCode * 397) ^ (Credentials != null ? Credentials.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(WebProxyExt left, WebProxyExt right)
    {    // If both are null, or both are same instance, return true.
        if (ReferenceEquals(left, right)) return true;

        // If one is null, but not both, return false.
        if (left is null || right is null) return false;

        return left.Equals(right);
    }

    public static bool operator !=(WebProxyExt left, WebProxyExt right)
    {
        return !(left == right);
    }
}