namespace FclEx.Http.Extensions;

public class HttpClientExtensionsTests
{
    [Fact]
    public void GetHandler_ReturnsRootHandlerStoredByHttpClient()
    {
        var handler = new TerminalHandler();
        using var client = new HttpClient(handler, disposeHandler: true);

        var actual = client.GetHandler();

        Assert.Same(handler, actual);
    }

    [Fact]
    public void GetPrimaryHandler_WhenClientUsesDelegatingHandlers_ReturnsLastHandlerInChain()
    {
        var primary = new TerminalHandler();
        var inner = new PassThroughHandler
        {
            InnerHandler = primary,
        };
        var outer = new PassThroughHandler
        {
            InnerHandler = inner,
        };
        using var client = new HttpClient(outer, disposeHandler: true);

        var actual = client.GetPrimaryHandler();

        Assert.Same(primary, actual);
    }

    [Fact]
    public void IgnoreRemoteCertificateValidation_WhenPrimaryHandlerIsHttpClientHandler_ConfiguresBypassCallback()
    {
        using var handler = new HttpClientHandler();
        using var client = new HttpClient(handler, disposeHandler: false);

        client.IgnoreRemoteCertificateValidation();

        Assert.Equal(ClientCertificateOption.Manual, handler.ClientCertificateOptions);
        Assert.NotNull(handler.ServerCertificateCustomValidationCallback);
        Assert.True(handler.ServerCertificateCustomValidationCallback(
            null!,
            null,
            null,
            SslPolicyErrors.RemoteCertificateNameMismatch));
    }

    [Fact]
    public void IgnoreRemoteCertificateValidation_WhenPrimaryHandlerIsSocketsHttpHandler_ConfiguresBypassCallback()
    {
        using var handler = new SocketsHttpHandler();
        using var client = new HttpClient(handler, disposeHandler: false);

        client.IgnoreRemoteCertificateValidation();

        Assert.NotNull(handler.SslOptions.RemoteCertificateValidationCallback);
        Assert.True(handler.SslOptions.RemoteCertificateValidationCallback(
            null!,
            null,
            null,
            SslPolicyErrors.RemoteCertificateNameMismatch));
    }

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
        var actual = HttpClientExtensions.FilterAndOrderIPAddresses(
            [IPAddress.IPv6Loopback, IPAddress.Loopback],
            policy);

        Assert.Equal(expectedFamilies, actual.Select(address => address.AddressFamily));
    }

    [Fact]
    public void FilterAndOrderIPAddresses_WhenNoAddressMatchesPolicy_ReturnsEmptyArray()
    {
        var actual = HttpClientExtensions.FilterAndOrderIPAddresses(
            [IPAddress.IPv6Loopback],
            IPVersionPolicy.OnlyIPv4);

        Assert.Empty(actual);
    }

    [Fact]
    public void FilterAndOrderIPAddresses_WhenPolicyIsUnknown_ThrowsArgumentOutOfRangeException()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            HttpClientExtensions.FilterAndOrderIPAddresses([IPAddress.Loopback], (IPVersionPolicy)999));

        Assert.Equal("policy", ex.ParamName);
    }

    [Fact]
    public async Task GetIPAddressesAsync_WhenLiteralAddressMatchesPolicy_ReturnsLiteralAddress()
    {
        var addresses = await HttpClientExtensions.GetIPAddressesAsync("127.0.0.1", IPVersionPolicy.OnlyIPv4, CancellationToken.None);

        Assert.Equal([IPAddress.Loopback], addresses);
    }

    [Fact]
    public async Task GetIPAddressesAsync_WhenPolicyIsUnknown_ThrowsArgumentOutOfRangeException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            HttpClientExtensions.GetIPAddressesAsync("127.0.0.1", (IPVersionPolicy)999, CancellationToken.None));

        Assert.Equal("policy", ex.ParamName);
    }

    [Fact]
    public async Task GetIPAddressesAsync_WhenLiteralAddressDoesNotMatchPolicy_ThrowsInvalidOperationException()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            HttpClientExtensions.GetIPAddressesAsync("127.0.0.1", IPVersionPolicy.OnlyIPv6, CancellationToken.None));

        Assert.Contains("OnlyIPv6", ex.Message);
        Assert.Contains("127.0.0.1", ex.Message);
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

            using var stream = await HttpClientExtensions.ConnectAsync(
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
    public void Create_ReturnsHttpClientWithDefaultClientTimeout()
    {
        var proxy = new WebProxy("http://127.0.0.1:8888");

        using var client = HttpClient.Create(new()
        {
            Proxy = proxy,
            ConnectTimeout = TimeSpan.FromSeconds(3),
        });

        Assert.Equal(TimeSpan.FromSeconds(100), client.Timeout);
    }

    [Fact]
    public void Create_UsesSocketsHttpHandlerConfiguredFromOptions()
    {
        var proxy = new WebProxy("http://127.0.0.1:8888");

        using var client = HttpClient.Create(new()
        {
            Proxy = proxy,
            AllowAutoRedirect = false,
            DisableServerCertificateValidation = true,
        });

        var handler = Assert.IsType<SocketsHttpHandler>(client.GetHandler());
        Assert.False(handler.AllowAutoRedirect);
        Assert.True(handler.UseProxy);
        Assert.Same(proxy, handler.Proxy);
        Assert.NotNull(handler.SslOptions.RemoteCertificateValidationCallback);
    }

    [RetryFact(5)]
    public async Task AddRetryPolicy_WhenExecutionTimeoutIsConfigured_TimesOutEachAttempt()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        const int retryCount = 2;
        var services = new ServiceCollection();

        var options = new HttpClientRetryPolicyOptions
        {
            ExecutionTimeout = timeout,
            RetryCount = 2,
            AutoUpdateTotalTimeout = true,
            SleepDurationProvider = m => TimeSpan.Zero,
        };

        services.AddHttpClient(string.Empty)
            // NOTE: to test HttpClient.Timeout, we need to make it less than SocketsHttpHandler.ConnectTimeout
            .ConfigurePrimaryHttpMessageHandler(() => HttpMessageHandler.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) }))
            .AddRetryPolicy(options);

        var provider = services.BuildServiceProvider();

        var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient();

        var watch = ValueStopwatch.StartNew();
        await Assert.ThrowsAnyAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://google.com:444/", HttpCompletionOption.ResponseHeadersRead));
        var time = watch.GetElapsedTime();

        var executeTime = timeout.Multiply(retryCount + 1);
        Assert.Equal(executeTime, time, TimeSpan.FromSeconds(0.5));
    }

    [RetryFact(5)]
    public async Task HttpClientTimeout_WhenConnectionDoesNotComplete_CancelsAfterConfiguredTimeout()
    {
        var timeout = TimeSpan.FromSeconds(0.2);
        var handler = HttpMessageHandler.CreateSocketsHttpHandler(new() { ConnectTimeout = TimeSpan.FromHours(1) });
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


    private sealed class PassThroughHandler : DelegatingHandler;

    private sealed class TerminalHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
