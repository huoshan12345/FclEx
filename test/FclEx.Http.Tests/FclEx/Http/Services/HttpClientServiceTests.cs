namespace FclEx.Http.Services;

public partial class HttpClientServiceTests(ITestOutputHelper output)
{
    public static readonly TheoryData<bool, bool, bool> AddCookieTestData = new[] { true, false }
            .CrossJoinCube()
            .ToTheoryData();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://localhost:1080")]
    public void Create_WithStringProxy_ConfiguresServiceProxy(string? proxy)
    {
        var http = HttpClientService.Create(proxy);
        Assert.Equal(WebProxy.Create(proxy).CastTo<WebProxy>().Address, http.Proxy.CastTo<WebProxy>()!.Address);
    }

    [Fact]
    public async Task SendAsync_WhenRequestSucceeds_ReturnsSuccessfulResponse()
    {
        var url = TestUrls.First();
        using var service = HttpClientService.Create(false);
        var response = await HttpRequest
            .Get(url)
            .SendAsync(service);
        Assert.False(response.IsError, () => response.Exception!.ToString());
    }

    [Theory]
    [MemberData(nameof(AddCookieTestData))]
    public void AddCookie_WhenCookieDomainDiffers_RequiresDomainOverride(bool useCookie, bool sameDomain, bool overrideDomain)
    {
        var uri = sameDomain
            ? new Uri("https://www.instagram.com/")
            : new Uri("https://www.google.com/");

        var cookies = SimpleCookies; // their domain is ".instagram.com"
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
        {
            if (useCookie == false || sameDomain || overrideDomain)
            {
                service.AddCookie(cookie, uri, overrideDomain);
            }
            else
            {
                Assert.ThrowsAny<CookieException>(() => service.AddCookie(cookie, uri));
            }

        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_WhenUriIsNull_DoesNotValidateCookieDomain(bool useCookie)
    {
        var cookies = SimpleCookies;
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, uri: null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAllCookies_ReturnsCookiesOnlyWhenCookieContainerIsEnabled(bool useCookie)
    {
        var cookies = SimpleCookies.Select(m => m.ToCookie()).ToDictionary(m => m.Name);
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Values)
            service.AddCookie(cookie, null);

        var actualCookies = service.GetAllCookies();
        if (useCookie)
        {
            Assert.Equal(cookies.Count, actualCookies.Count);
            foreach (var actualCookie in actualCookies)
            {
                Assert.True(cookies.TryGetValue(actualCookie.Name, out var cookie));
                Assert.NotNull(cookie);
                Assert.Equal(cookie.Value, actualCookie.Value);
                Assert.Equal(cookie.Domain, actualCookie.Domain);
            }
        }
        else
        {
            Assert.Empty(actualCookies);
        }
    }

    [Fact]
    public void GetFactory_WhenOptionsAreEquivalent_ReusesFactory()
    {
        var fac1 = GetFactory(new());
        var fac2 = GetFactory(new());
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
    }

    [Fact]
    public void GetProvider_RemovesDefaultLoggingHttpMessageHandlerBuilderFilter()
    {
        var provider = HttpClientService.GetProvider(new());
        var filter = provider.GetService<IHttpMessageHandlerBuilderFilter>();
        Assert.True(filter is null || filter.GetType().FullName != "Microsoft.Extensions.Http.LoggingHttpMessageHandlerBuilderFilter");
    }

    private static IHttpClientFactory GetFactory(HttpClientOptions options)
    {
        var provider = HttpClientService.GetProvider(options);
        return provider.GetRequiredService<IHttpClientFactory>();
    }

    private static void CheckProxy(HttpClient client, IWebProxy? proxy)
    {
        var handler = client.GetHandler()
            .EnumerateInner()
            .OfType<SocketsHttpHandler>()
            .FirstOrDefault();
        Assert.NotNull(handler);
        var webProxy = handler.Proxy.CastTo<IWebProxy>();
        Assert.Equal<IWebProxy>(proxy, webProxy, WebProxyInterfaceEqualityComparer.Instance);
    }

    [Fact]
    public void GetFactory_WhenProxyOptionsAreEquivalent_ReusesFactory()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var fac1 = GetFactory(new() { HandlerOptions = new() { Proxy = WebProxy.Create(uri) } });
        var fac2 = GetFactory(new() { HandlerOptions = new() { Proxy = WebProxy.Create(uri) } });
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckProxy(fac1.CreateClient(), WebProxy.Create(uri));
        CheckProxy(fac2.CreateClient(), WebProxy.Create(uri));
    }

    [Fact]
    public void ProxySetter_RebuildsClientContextWithNewProxy()
    {
        var http = HttpClientService.Create();
        {
            var client = http.CreateHttpClientContext().Client;
            Assert.Null(http.Proxy);
            CheckProxy(client, null);
        }
        {
            var proxy = WebProxy.Create("http://127.0.0.1:8888");
            http.Proxy = proxy;
            var client = http.CreateHttpClientContext().Client;
            CheckProxy(client, proxy);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateHttpClientContext_WhenHttpClientProviderIsUsed_UsesConfiguredDisposeFlag(bool disposeHttpClient)
    {
        using var client = new TrackingHttpClient(new CaptureHandler());
        using var service = HttpClientService.Create(
            () => client,
            disposeHttpClient,
            options: new()
            {
                RetryPolicyOptions = new()
                {
                    RetryCount = 0,
                },
            },
            useCookie: false);

        var context = service.CreateHttpClientContext();

        Assert.Same(client, context.Client);
        Assert.Equal(disposeHttpClient, context.DisposeHttpClient);

        context.Dispose();

        Assert.Equal(disposeHttpClient, client.Disposed);
    }

    [Fact]
    public void GetFactory_WhenProxyOptionsDiffer_CreatesDifferentFactories()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var fac1 = GetFactory(new() { HandlerOptions = new() { Proxy = WebProxy.Create(uri) } });
        var fac2 = GetFactory(new() { HandlerOptions = new() { Proxy = null } });
        Assert.NotEqual(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckProxy(fac1.CreateClient(), WebProxy.Create(uri));
        CheckProxy(fac2.CreateClient(), null);
    }

    [Fact]
    public void GetFactory_WhenCertificateValidationOptionsDiffer_CreatesDifferentFactories()
    {
        var fac1 = GetFactory(new() { HandlerOptions = new() { DisableServerCertificateValidation = true } });
        var fac2 = GetFactory(new() { HandlerOptions = new() { DisableServerCertificateValidation = false } });

        Assert.NotEqual(fac1, fac2, ReferenceEqualityComparer.Instance);
    }

    [RetryTheory(DisableParallelization = true)]
    [InlineData(1, 0.1)]
    [InlineData(2, 0.1)]
    [InlineData(2, 0.2)]
    public async Task CreateHttpClientContext_AppliesRetryPolicyTimeoutForEachAttempt(int retryCount, double timeoutSeconds)
    {
        var expectedTime = TimeSpan.FromSeconds(timeoutSeconds).Multiply(retryCount + 1);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var http = HttpClientService.Create(m =>
        {
            m.HandlerOptions.ConnectTimeout = TimeSpan.FromMinutes(1);
            m.RetryPolicyOptions.SleepDurationProvider = _ => TimeSpan.Zero;
            m.RetryPolicyOptions.RetryCount = retryCount;
        });
        var response = await HttpRequest.Get("https://google.com:444/")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http);

        Assert.True(response.IsError);
        output.WriteLine(response.Exception.ToString());

        Assert.IsType<TaskCanceledException>(response.Exception);
        Assert.Equal(expectedTime, response.Elapsed, TimeSpan.FromSeconds(0.2));
    }

    private sealed class TrackingHttpClient(HttpMessageHandler handler) : HttpClient(handler)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
