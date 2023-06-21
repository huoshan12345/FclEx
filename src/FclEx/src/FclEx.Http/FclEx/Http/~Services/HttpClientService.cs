using System.Net.Sockets;
using MoreLinq;

namespace FclEx.Http;

public class HttpClientService : AbstractHttpClientService
{
    public static HttpClientService Default { get; } = new(false);

    protected volatile HttpClient _httpClient;

    private static HttpClient CreateHttpClient(IWebProxy? proxy)
    {
        var handler = new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            ConnectTimeout = TimeSpan.FromMinutes(1),
            MaxConnectionsPerServer = int.MaxValue,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            Proxy = null,
            UseCookies = false,
            UseProxy = false,
            ConnectCallback = async (context, cancellationToken) =>
            {
                // Use DNS to look up the IP addresses of the target host:
                // - IP v4: AddressFamily.InterNetwork
                // - IP v6: AddressFamily.InterNetworkV6
                // - IP v4 or IP v6: AddressFamily.Unspecified
                // note: this method throws a SocketException when there is no IP address for the host
                var ips = IPAddress.TryParse(context.DnsEndPoint.Host, out var ip)
                    ? new[] { ip }
                    : (await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.Unspecified, cancellationToken)).AddressList;
                
                // Open the connection to the target host/port
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                {
                    // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
                    NoDelay = true
                };

                Exception? lastEx = null;
                foreach (var address in ips.OrderBy(m => m.AddressFamily)) // make sure ipv4 addresses are preferred
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

        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

        var httpClient = new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
        httpClient.DefaultRequestHeaders.Add(HttpKnownHeaderNames.UserAgent, HttpConstants.DefaultUserAgent);
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        return httpClient;
    }

    protected override void SetProxy(IWebProxy? proxy)
    {
        if (Equals(_webProxy, proxy))
            return;

        _webProxy = proxy;
        _httpClient = CreateHttpClient(_webProxy);
    }

    protected override Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token)
    {
        return ExecuteAsyncInternal(_httpClient, request, response, token);
    }

    public HttpClientService(bool useCookie = true, IWebProxy? proxy = null, ILoggerFactory? loggerFactory = null)
        : base(useCookie, proxy, loggerFactory)
    {
        _httpClient = CreateHttpClient(_webProxy);
    }

    public override void Dispose()
    {
    }
}