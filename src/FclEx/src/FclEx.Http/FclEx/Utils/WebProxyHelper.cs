namespace FclEx.Utils;

public class WebProxyHelper
{
    public static IWebProxy Create(Uri? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
    {
        if (address is { UserInfo: { Length: > 0 } userInfo } && credentials == null)
        {
            var (user, pass) = userInfo.SplitTwo(":");
            credentials = new NetworkCredential(user.UriUnescape(), pass.UriUnescape());
            var builder = new UriBuilder(address.Scheme, address.Host, address.Port, address.AbsolutePath, address.Query) { Fragment = address.Fragment };
            address = builder.Uri;
        }

        return new WebProxy(address, bypassOnLocal, bypassList, credentials);
    }

    public static IWebProxy Create(string? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
        => Create(address.IsNullOrEmpty() ? null : new Uri(address, UriKind.Absolute), bypassOnLocal, bypassList, credentials);
}