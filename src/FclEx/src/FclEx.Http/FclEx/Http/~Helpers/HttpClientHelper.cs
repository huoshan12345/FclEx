using System.Net.Sockets;
using static FclEx.Http.IPVersionOption;

namespace FclEx.Http;

public static class HttpClientHelper
{
    public static async Task<string> GetPublicIpAsync(HttpClient httpClient)
    {
        using var res = await httpClient.GetAsync("http://ip4only.me/api/", HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();

        var str = await res.Content.ReadAsStringAsync();
        /*
            Example output
            IPv4,192.0.2.60,v1.1,,,See http://ip6.me/docs/ for api documentation
            IPv6,2001:db8:0:0:8:800:200c:417a,v1.1,,,See http://ip6.me/docs for api documentation
        */
        var ip = str.Split(',')[1];
        return ip;
    }

    public static SocketsHttpHandler CreateSocketsHttpHandler(SocketsHttpHandlerOptions? options = null)
    {
        options ??= SocketsHttpHandlerOptions.Default;
        return new SocketsHttpHandler
        {
            ConnectTimeout = options.ConnectTimeout,
            PooledConnectionLifetime = options.PooledConnectionLifetime, // The connection is reestablished periodically to reflect the DNS or other network changes.
            PooledConnectionIdleTimeout = options.PooledConnectionIdleTimeout,
            MaxConnectionsPerServer = int.MaxValue,
            UseCookies = false,
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.All,
            UseProxy = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var host = context.DnsEndPoint.Host;
                var family = options.IpVersionOption switch
                {
                    OnlyIPv4 => AddressFamily.InterNetwork,
                    OnlyIPv6 => AddressFamily.InterNetworkV6,
                    PreferIPv4 => AddressFamily.Unspecified,
                    PreferIPv6 => AddressFamily.Unspecified,
                    _ => throw new ArgumentOutOfRangeException(nameof(options.IpVersionOption), options.IpVersionOption, null)
                };

                // Use DNS to look up the IP addresses of the target host:
                // - IP v4: AddressFamily.InterNetwork
                // - IP v6: AddressFamily.InterNetworkV6
                // - IP v4 or IP v6: AddressFamily.Unspecified
                // note: this method throws a SocketException when there is no IP address for the host
                var ips = IPAddress.TryParse(host, out var ip)
                    ? new[] { ip }
                    : (await Dns.GetHostEntryAsync(host, family, cancellationToken)).AddressList;

                if (ips.IsEmpty())
                {
                    throw new InvalidOperationException($"Cannot get any ipv4 addresses whose family is {family} for {host}");
                }

                // Open the connection to the target host/port
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
                    NoDelay = true
                };

                var desc = options.IpVersionOption is PreferIPv6 or OnlyIPv6;
                Exception? lastEx = null;
                foreach (var address in ips.OrderBy(m => m.AddressFamily, desc)) // make sure ipv4 addresses are preferred
                {
                    try
                    {
                        await socket.ConnectAsync(address, context.DnsEndPoint.Port, cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                    }
                }

                socket.Dispose();
                throw lastEx!; // should not be null here.
            }
        };
    }
}