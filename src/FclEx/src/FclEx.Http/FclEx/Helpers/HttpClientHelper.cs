using System.Net.Sockets;

namespace FclEx.Helpers;

public enum IpVersionPreference
{
    OnlyIpV4,
    OnlyIpV6,
    PreferIpV4,
    PreferIpV6,
}

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

    public static HttpMessageHandler CreateSocketsHttpHandler(TimeSpan? connectTimeout = null, IpVersionPreference preference = IpVersionPreference.PreferIpV4)
    {
#if NETSTANDARD2_0
        return new StandardSocketsHttpHandler
#else
        return new SocketsHttpHandler
#endif
        {
            ConnectTimeout = connectTimeout ?? TimeSpan.FromSeconds(10),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2), // The connection is reestablished periodically to reflect the DNS or other network changes.
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = ushort.MaxValue,
            UseCookies = false,
            AllowAutoRedirect = true,
            UseProxy = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                var host = context.DnsEndPoint.Host;
                var family = preference switch
                {
                    IpVersionPreference.OnlyIpV4 => AddressFamily.InterNetwork,
                    IpVersionPreference.OnlyIpV6 => AddressFamily.InterNetworkV6,
                    IpVersionPreference.PreferIpV4 => AddressFamily.Unspecified,
                    IpVersionPreference.PreferIpV6 => AddressFamily.Unspecified,
                    _ => throw new ArgumentOutOfRangeException(nameof(preference), preference, null)
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

                var desc = preference is IpVersionPreference.PreferIpV6 or IpVersionPreference.OnlyIpV6;
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