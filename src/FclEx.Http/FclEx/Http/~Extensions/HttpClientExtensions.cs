using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static FclEx.Http.IPVersionPolicy;

namespace FclEx.Http;

/// <summary>
/// Called when an HTTP response has an unsuccessful status code and its content has been read.
/// </summary>
public delegate void OnHttpFailedCode(HttpResponseMessage response, string content);

/// <summary>
/// Extensions for inspecting and modifying <see cref="HttpClient"/> handler behavior.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Gets the root message handler stored by <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// This method reads a private runtime field inherited from <see cref="HttpMessageInvoker"/>. It is useful
    /// for diagnostics and tests, but may break if runtime internals change and may not be suitable for trimming
    /// or AOT scenarios.
    /// </remarks>
    public static HttpMessageHandler GetHandler(this HttpClient httpClient)
    {
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<HttpMessageHandler>(httpClient);
    }

    /// <summary>
    /// Gets the last non-delegating handler in the <see cref="HttpClient"/> handler chain.
    /// </summary>
    /// <remarks>
    /// This method depends on <see cref="GetHandler(HttpClient)"/> and therefore reads a private runtime field.
    /// It is best suited for diagnostics and tests rather than application control flow.
    /// </remarks>
    public static HttpMessageHandler GetPrimaryHandler(this HttpClient httpClient)
    {
        var handler = httpClient.GetHandler();

        var p = handler;
        while (true)
        {
            var next = (p as DelegatingHandler)?.InnerHandler;
            if (next == null)
                return p;

            p = next;
        }
    }

    /// <summary>
    /// Disables remote server certificate validation on the primary handler of an existing client when the primary handler type supports it.
    /// This mutates <see cref="SocketsHttpHandler"/> or <see cref="HttpClientHandler"/> instances and has no effect for other primary handler types.
    /// </summary>
    public static void IgnoreRemoteCertificateValidation(this HttpClient httpClient)
    {
        var handler = httpClient.GetPrimaryHandler();
        switch (handler)
        {
            case SocketsHttpHandler socketsHttpHandler:
                socketsHttpHandler.SslOptions.RemoteCertificateValidationCallback = HttpClient.BypassServerCertificateValidation;
                break;
            case HttpClientHandler httpClientHandler:
                httpClientHandler.ServerCertificateCustomValidationCallback = HttpClient.BypassServerCertificateValidation;
                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                break;
        }
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

    extension(HttpClient)
    {
        /// <summary>
        /// Creates an <see cref="HttpClient"/> backed by a configured <see cref="SocketsHttpHandler"/>.
        /// </summary>
        /// <param name="options">Handler options. When <see langword="null"/>, default <see cref="SocketsHttpHandlerOptions"/> values are used.</param>
        /// <returns>A client that owns the created handler.</returns>
        public static HttpClient Create(SocketsHttpHandlerOptions? options = null)
        {
            return new(HttpMessageHandler.CreateSocketsHttpHandler(options));
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


    }
}
