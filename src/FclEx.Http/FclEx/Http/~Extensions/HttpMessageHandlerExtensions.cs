namespace FclEx.Http;

/// <summary>
/// Extensions for inspecting HTTP message handler chains.
/// </summary>
public static class HttpMessageHandlerExtensions
{
    /// <summary>
    /// Enumerates a handler and each nested <see cref="DelegatingHandler.InnerHandler"/> until the primary handler is reached.
    /// </summary>
    public static IEnumerable<HttpMessageHandler> EnumerateInner(this HttpMessageHandler handler)
    {
        var p = handler;
        while (p != null)
        {
            yield return p;

            if (p is DelegatingHandler delegatingHandler)
                p = delegatingHandler.InnerHandler;
            else
                break;
        }
    }

    // NOTE: do not put CreateSocketsHttpHandler into SocketsHttpHandlerExtensions because it is not convenient to call SocketsHttpHandler.Create() on .netstandard2.0
    extension(HttpMessageHandler)
    {
        /// <summary>
        /// Creates a <see cref="SocketsHttpHandler"/> configured from <see cref="SocketsHttpHandlerOptions"/>.
        /// </summary>
        /// <param name="options">Handler options. When <see langword="null"/>, default options are used.</param>
        /// <returns>
        /// A handler with cookie handling disabled, automatic redirects and decompression copied from the options,
        /// optional proxy support, optional certificate validation bypass, and a connect callback that applies
        /// <see cref="SocketsHttpHandlerOptions.IPVersionPolicy"/>.
        /// </returns>
        public static SocketsHttpHandler CreateSocketsHttpHandler(SocketsHttpHandlerOptions? options = null)
        {
            options ??= new();
            return new SocketsHttpHandler
            {
                ConnectTimeout = options.ConnectTimeout,
                PooledConnectionLifetime = options.PooledConnectionLifetime, // The connection is reestablished periodically to reflect the DNS or other network changes.
                PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout,
                MaxConnectionsPerServer = int.MaxValue,
                UseCookies = false,
                AllowAutoRedirect = options.AllowAutoRedirect,
                AutomaticDecompression = options.AutomaticDecompression,
                UseProxy = options.Proxy is not null,
                Proxy = options.Proxy,
#if NET6_0_OR_GREATER
                EnableMultipleHttp2Connections = options.EnableMultipleHttp2Connections,
#endif
                SslOptions = new()
                {
                    RemoteCertificateValidationCallback = options.DisableServerCertificateValidation
                        ? HttpClient.BypassServerCertificateValidation
                        : null, // if DisableServerCertificateValidation is false, use the default validation callback (which is null)
                },
                ConnectCallback = async (context, token) =>
                {
                    var ips = await HttpClientExtensions.GetIPAddressesAsync(context.DnsEndPoint.Host, options.IPVersionPolicy, token);
                    return await HttpClientExtensions.ConnectAsync(context.DnsEndPoint, ips, token);
                }
            };
        }
    }
}
