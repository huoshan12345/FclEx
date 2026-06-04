using System.Net.Sockets;

namespace FclEx.Http.Helpers;

public class HttpClientHelperTests
{
    public static TheoryData<IPVersionPolicy, AddressFamily[]> IPAddressOrderCases { get; } = new()
    {
        { IPVersionPolicy.OnlyIPv4, [AddressFamily.InterNetwork] },
        { IPVersionPolicy.OnlyIPv6, [AddressFamily.InterNetworkV6] },
        { IPVersionPolicy.PreferIPv4, [AddressFamily.InterNetwork, AddressFamily.InterNetworkV6] },
        { IPVersionPolicy.PreferIPv6, [AddressFamily.InterNetworkV6, AddressFamily.InterNetwork] },
    };

    [Theory]
    [MemberData(nameof(IPAddressOrderCases))]
    public void FilterAndOrderIPAddresses_ReturnsAddressesForPolicy(IPVersionPolicy policy, AddressFamily[] expectedFamilies)
    {
        var actual = HttpClientHelper.FilterAndOrderIPAddresses(
            [IPAddress.IPv6Loopback, IPAddress.Loopback],
            policy);

        Assert.Equal(expectedFamilies, actual.Select(address => address.AddressFamily));
    }

    [Fact]
    public async Task ConnectAsync_WhenFirstAddressFails_ConnectsToNextAddressWithNewSocket()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = listener.LocalEndpoint.CastTo<IPEndPoint>().Port;
            var acceptTask = listener.AcceptSocketAsync();

            using var stream = await HttpClientHelper.ConnectAsync(
                new DnsEndPoint("localhost", port),
                [IPAddress.IPv6Loopback, IPAddress.Loopback],
                CancellationToken.None);
            using var socket = await acceptTask;

            var remoteEndPoint = Assert.IsType<IPEndPoint>(socket.RemoteEndPoint);
            Assert.True(stream.CanWrite);
            Assert.Equal(AddressFamily.InterNetwork, remoteEndPoint.AddressFamily);
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public void CreateSocketsHttpHandler_ByDefault_UsesDefaultServerCertificateValidation()
    {
        using var handler = HttpClientHelper.CreateSocketsHttpHandler();

        Assert.Null(handler.SslOptions.RemoteCertificateValidationCallback);
    }

    [Fact]
    public void CreateSocketsHttpHandler_WhenCertificateValidationIsDisabled_BypassesServerCertificateValidation()
    {
        using var handler = HttpClientHelper.CreateSocketsHttpHandler(new()
        {
            DisableServerCertificateValidation = true,
        });

        var callback = handler.SslOptions.RemoteCertificateValidationCallback;

        Assert.NotNull(callback);
        Assert.True(callback(null!, null, null, SslPolicyErrors.RemoteCertificateNameMismatch));
    }

    [RetryFact(5)]
    public async Task GetRetryPolicy_Timeout_Test()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        const int retryCount = 2;
        var services = new ServiceCollection();

        services.AddHttpClient(string.Empty)
            // NOTE: to test HttpClient.Timeout, we need to make it less than SocketsHttpHandler.ConnectTimeout
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) }))
            .AddRetryPolicy(timeout, 2, true, m => TimeSpan.Zero);

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        await Assert.ThrowsAnyAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        var executeTime = timeout.Multiply(retryCount + 1);
        Assert.Equal(executeTime, time, TimeSpan.FromSeconds(0.5));
    }

    [RetryFact(5)]
    public async Task HttpClient_Timeout_Test()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        var handler = HttpClientHelper.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) });
        // ReSharper disable once ShortLivedHttpClient
        using var httpClient = new HttpClient(handler, true) { Timeout = timeout };

        var watch = ValueStopwatch.StartNew();
        var ex = await Assert.ThrowsAnyAsync<TaskCanceledException>(() => httpClient.GetAsync("https://google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

#if NET5_0_OR_GREATER
        Assert.Contains("configured HttpClient.Timeout", ex.Message);
        Assert.NotNull(ex.InnerException);
#endif
        Assert.Equal(timeout, time, TimeSpan.FromSeconds(0.5));
    }
}
