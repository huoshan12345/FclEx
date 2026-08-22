namespace FclEx.Http;

public static class WebProxyExtensions
{
    private static readonly WebProxy Empty = WebProxy.Create((Uri?)null);

    extension(WebProxy)
    {
        /// <summary>
        /// Creates a proxy from a URI and optional proxy settings.
        /// </summary>
        /// <param name="address">The proxy address, or <see langword="null"/> for an empty proxy.</param>
        /// <param name="bypassOnLocal">Whether local addresses should bypass the proxy.</param>
        /// <param name="bypassList">Address patterns that bypass the proxy.</param>
        /// <param name="credentials">
        /// Explicit proxy credentials. When this is <see langword="null"/> and <paramref name="address"/> contains user info,
        /// that user info is converted to <see cref="NetworkCredential"/> and removed from the proxy address.
        /// </param>
        /// <returns>A configured <see cref="WebProxy"/>.</returns>
        public static WebProxy Create(Uri? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
        {
            // ReSharper disable once InvertIf
            if (address is { UserInfo: { Length: > 0 } userInfo } && credentials == null)
            {
                var (user, pass) = userInfo.Partition(":");
                credentials = new NetworkCredential(user.UriUnescape(), pass.UriUnescape());
                var builder = new UriBuilder(address.Scheme, address.Host, address.Port, address.AbsolutePath, address.Query) { Fragment = address.Fragment };
                address = builder.Uri;
            }

            return new WebProxy(address, bypassOnLocal, bypassList, credentials);
        }

        /// <summary>
        /// Creates a proxy from an absolute URI string.
        /// </summary>
        /// <param name="address">The proxy address string. <see langword="null"/> and empty strings create an empty proxy.</param>
        /// <param name="bypassOnLocal">Whether local addresses should bypass the proxy.</param>
        /// <param name="bypassList">Address patterns that bypass the proxy.</param>
        /// <param name="credentials">Explicit proxy credentials.</param>
        /// <returns>A configured <see cref="WebProxy"/>.</returns>
        public static WebProxy Create(string? address, bool bypassOnLocal = false, string[]? bypassList = null, ICredentials? credentials = null)
            => WebProxy.Create(address.IsNullOrEmpty() ? null : new Uri(address, UriKind.Absolute), bypassOnLocal, bypassList, credentials);

        /// <summary>
        /// A reusable empty proxy instance whose address and credentials are not set.
        /// </summary>
        public static WebProxy Empty => Empty;
    }
}
