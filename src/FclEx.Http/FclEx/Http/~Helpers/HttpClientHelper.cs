using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static FclEx.Http.IPVersionPolicy;

namespace FclEx.Http;

/// <summary>
/// Creates <see cref="HttpClient"/> and <see cref="SocketsHttpHandler"/> instances from FclEx HTTP options.
/// </summary>
public static class HttpClientHelper
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by a configured <see cref="SocketsHttpHandler"/>.
    /// </summary>
    /// <param name="options">Handler options. When <see langword="null"/>, default <see cref="SocketsHttpHandlerOptions"/> values are used.</param>
    /// <returns>A client that owns the created handler.</returns>
    public static HttpClient Create(SocketsHttpHandlerOptions? options = null)
    {
        return new(CreateSocketsHttpHandler(options));
    }

    /// <summary>
    /// A certificate validation callback that accepts every server certificate.
    /// </summary>
    /// <returns>Always <see langword="true"/>.</returns>
    /// <remarks>Use only when certificate validation has explicitly been disabled.</remarks>
    public static bool BypassServerCertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

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
                    ? BypassServerCertificateValidation
                    : null, // if DisableServerCertificateValidation is false, use the default validation callback (which is null)
            },
            ConnectCallback = async (context, token) =>
            {
                var ips = await GetIPAddressesAsync(context.DnsEndPoint.Host, options.IPVersionPolicy, token);
                return await ConnectAsync(context.DnsEndPoint, ips, token);
            }
        };
    }

    internal static async Task<IPAddress[]> GetIPAddressesAsync(string host, IPVersionPolicy policy, CancellationToken token)
    {
#if NET5_0_OR_GREATER
        var family = policy switch
        {
            OnlyIPv4 => AddressFamily.InterNetwork,
            OnlyIPv6 => AddressFamily.InterNetworkV6,
            PreferIPv4 or PreferIPv6 => AddressFamily.Unspecified,
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
        };
#endif

        // Use DNS to look up the IP addresses of the target host:
        // - IP v4: AddressFamily.InterNetwork
        // - IP v6: AddressFamily.InterNetworkV6
        // - IP v4 or IP v6: AddressFamily.Unspecified
        // note: this method throws a SocketException when there is no IP address for the host
        var ips = IPAddress.TryParse(host, out var ip)
            ? [ip]
#if !NET5_0_OR_GREATER
            : (await Dns.GetHostEntryAsync(host)).AddressList;
#else
            : (await Dns.GetHostEntryAsync(host, family, token)).AddressList;
#endif

        var orderedIps = FilterAndOrderIPAddresses(ips, policy);
        return orderedIps.IsEmpty()
            ? throw new InvalidOperationException($"Cannot get any IP addresses whose family matches {policy} for {host}")
            : orderedIps;
    }

    internal static IPAddress[] FilterAndOrderIPAddresses(IEnumerable<IPAddress> addresses, IPVersionPolicy policy)
    {
        var filtered = policy switch
        {
            OnlyIPv4 => addresses.Where(address => address.AddressFamily == AddressFamily.InterNetwork),
            OnlyIPv6 => addresses.Where(address => address.AddressFamily == AddressFamily.InterNetworkV6),
            PreferIPv4 or PreferIPv6 => addresses,
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
        };
        var preferIPv6 = policy is PreferIPv6 or OnlyIPv6;
        return filtered.OrderBy(address => address.AddressFamily, preferIPv6).ToArray();
    }

    internal static async Task<NetworkStream> ConnectAsync(DnsEndPoint endpoint, IEnumerable<IPAddress> addresses, CancellationToken token)
    {
        Exception? lastEx = null;
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                // Turn off Nagle algorithm since it degrades performance in most HttpClient scenarios.
                NoDelay = true,
            };

            try
            {
                await socket.ConnectAsync(address, endpoint.Port, token);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception ex)
            {
                socket.Dispose();
                lastEx = ex;

                if (ex is OperationCanceledException canceledException
                    && canceledException.CancellationToken == token
                    && token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        lastEx?.ReThrow();

        // This should never happen since the caller will throw a SocketException when there is no IP address for the host, but just in case.
        throw new InvalidOperationException($"Cannot connect to {endpoint}.");
    }
}
