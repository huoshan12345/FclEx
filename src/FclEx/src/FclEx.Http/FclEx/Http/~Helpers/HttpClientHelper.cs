using static FclEx.Http.IPVersionPolicy;

namespace FclEx.Http;

public static class HttpClientHelper
{
    public static HttpClient Create(SocketsHttpHandlerOptions? options = null)
    {
        return new(CreateSocketsHttpHandler(options));
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
            UseProxy = options.Proxy is not null,
            Proxy = options.Proxy,
            EnableMultipleHttp2Connections = options.EnableMultipleHttp2Connections,
            SslOptions = new()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true,
            },
            ConnectCallback = async (context, token) =>
            {
                var host = context.DnsEndPoint.Host;
                var family = options.IPVersionPolicy switch
                {
                    OnlyIPv4 => AddressFamily.InterNetwork,
                    OnlyIPv6 => AddressFamily.InterNetworkV6,
                    PreferIPv4 => AddressFamily.Unspecified,
                    PreferIPv6 => AddressFamily.Unspecified,
                    _ => throw new ArgumentOutOfRangeException(nameof(options.IPVersionPolicy), options.IPVersionPolicy, null)
                };

                // Use DNS to look up the IP addresses of the target host:
                // - IP v4: AddressFamily.InterNetwork
                // - IP v6: AddressFamily.InterNetworkV6
                // - IP v4 or IP v6: AddressFamily.Unspecified
                // note: this method throws a SocketException when there is no IP address for the host
                var ips = IPAddress.TryParse(host, out var ip)
                    ? [ip]
                    : (await Dns.GetHostEntryAsync(host, family, token)).AddressList;

                if (ips.IsEmpty())
                {
                    throw new InvalidOperationException($"Cannot get any ipv4 addresses whose family is {family} for {host}");
                }

                // Open the connection to the target host/port
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    // Turn off Nagle algorithm since it degrades performance in most HttpClient scenarios.
                    NoDelay = true,
                };

                var desc = options.IPVersionPolicy is PreferIPv6 or OnlyIPv6;
                Exception? lastEx = null;
                foreach (var address in ips.OrderBy(m => m.AddressFamily, desc)) // make sure ipv4 addresses are preferred
                {
                    try
                    {
                        await socket.ConnectAsync(address, context.DnsEndPoint.Port, token);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                    }
                }

                socket.Dispose();

                lastEx!.ReThrow(); // should not be null here.
                return default;
            }
        };

        static async Task CheckSocketConnection(Socket s)
        {
            /*
                s.Poll returns true if
                    connection is closed, reset, terminated or pending (meaning no active connection)
                    connection is active and there is data available for reading

                s.Available returns number of bytes available for reading

                if both are true:
                    there is no data available to read so connection is not active
            */
            var part1 = s.Poll(1000, SelectMode.SelectRead);
            var part2 = s.Available == 0;
            if (part1 && part2)
                throw new InvalidOperationException("There is no data available to read from socket.");

            var sentBytesCount = await s.SendAsync(new ArraySegment<byte>(new byte[1], 1, 0), SocketFlags.None);
            if (sentBytesCount != 1)
                throw new InvalidOperationException("Cannot send any data via socket.");
        }
    }

    private static readonly FieldInfo _underlyingHandler =
            typeof(HttpClientHandler).GetRequiredField("_underlyingHandler");

    // NOTE: we only change some properties of the handler instead of creating a new one, in order to keep the settings made in other packages.
    // e.g. Matrix client adds a certificate into HttpClientHandler, which should be kept.
    private static void ConfigureHttpMessageHandler(HttpMessageHandler handler)
    {
        // NOTE: HttpClient use HttpClientHandler as the primary handler, but other types may be used like SocketsHttpHandler.
        switch (handler)
        {
            case HttpClientHandler httpClientHandler:
                ConfigureHttpClientHandler(httpClientHandler);
                break;

            case SocketsHttpHandler socketsHttpHandler:
                ConfigureSocketsHttpHandler(socketsHttpHandler);
                break;
        }
    }

    private static void ConfigureHttpClientHandler(HttpClientHandler handler)
    {
        var inner = _underlyingHandler.GetValue(handler);

        if (inner is SocketsHttpHandler socketsHttpHandler)
        {
            ConfigureSocketsHttpHandler(socketsHttpHandler);
        }
        // NOTE: the underlying handler may also be BrowserHttpHandler, which is probably used in blazor.
    }

    private static void ConfigureSocketsHttpHandler(SocketsHttpHandler handler)
    {
        // The connection is reestablished periodically to reflect the DNS or other network changes.
        handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2);
        handler.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1);
        handler.ConnectTimeout = TimeSpan.FromSeconds(10);
        handler.MaxConnectionsPerServer = ushort.MaxValue;
        handler.UseCookies = false;
        handler.AllowAutoRedirect = true;
        handler.UseProxy = false;
    }
}