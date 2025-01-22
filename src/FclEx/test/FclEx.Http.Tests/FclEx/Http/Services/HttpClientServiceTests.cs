namespace FclEx.Http.Services;

public partial class HttpClientServiceTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://localhost:1080")]
    public void Constructor_Test(string? proxy)
    {
        var http = HttpClientService.Create(proxy);
        Assert.Equal(WebProxyHelper.Create(proxy).CastTo<WebProxy>().Address, http.Proxy.CastTo<WebProxy>()!.Address);
    }

    [Fact]
    public async Task SendAsync_Success()
    {
        using var service = HttpClientService.Create(false);
        for (var i = 0; i < 5; i++)
        {
            var res = await HttpRequest.Get("https://www.baidu.com")
                .SendAsync(service);
            AssertEx.False(res.HasError, () => res.Exception!.ToString());
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_Test(bool useCookie)
    {
        var uri = new Uri("https://www.instagram.com/");
        var cookies = SimpleCookies;
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, uri);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_NullUri_Test(bool useCookie)
    {
        var cookies = SimpleCookies;
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAllCookies_Test(bool useCookie)
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
    public void GetFactory_Default_Test()
    {
        var fac1 = GetFactory(HttpClientOptions.Default);
        var fac2 = GetFactory(HttpClientOptions.Default);
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
    }

    [Fact]
    public void LoggingHttpMessageHandlerBuilderFilter_Remove_Test()
    {
        var provider = HttpClientService.GetProvider(HttpClientOptions.Default);
        var filter = provider.GetService<IHttpMessageHandlerBuilderFilter>();
        Assert.True(filter is null || filter.GetType().FullName != "Microsoft.Extensions.Http.LoggingHttpMessageHandlerBuilderFilter");
    }

    private static IHttpClientFactory GetFactory(HttpClientOptions options)
    {
        var provider = HttpClientService.GetProvider(options);
        return provider.GetRequiredService<IHttpClientFactory>();
    }

    private static void CheckProxy(HttpMessageInvoker client, IWebProxy? proxy)
    {
        var handler = client.GetHandler()
            .EnumerateInner()
            .OfType<SocketsHttpHandler>()
            .FirstOrDefault();
        Assert.NotNull(handler);
        var webProxy = handler.Proxy.CastTo<IWebProxy>();
        Assert.Equal<IWebProxy>(proxy, webProxy, IWebProxyEqualityComparer.Instance);
    }



    [Fact]
    public void GetFactory_Proxy_Test()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var fac1 = GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        var fac2 = GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckProxy(fac1.CreateClient(), WebProxyHelper.Create(uri));
        CheckProxy(fac2.CreateClient(), WebProxyHelper.Create(uri));
    }

    [Fact]
    public void SetProxy_Test()
    {
        var http = HttpClientService.Create();
        {
            var client = http.CreateHttpClientContext().Client;
            Assert.Null(http.Proxy);
            CheckProxy(client, null);
        }
        {
            var proxy = WebProxyHelper.Create("http://127.0.0.1:8888");
            http.Proxy = proxy;
            var client = http.CreateHttpClientContext().Client;
            Assert.Equal(proxy, http.Proxy);
            CheckProxy(client, proxy);
        }
    }

    [Fact]
    public void GetFactory_Proxy_NotSame()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var fac1 = GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        var fac2 = GetFactory(new() { Proxy = null });
        Assert.NotEqual(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckProxy(fac1.CreateClient(), WebProxyHelper.Create(uri));
        CheckProxy(fac2.CreateClient(), null);
    }

    [LocalOnlyTheory]
    [InlineData(1, 0.1)]
    [InlineData(2, 0.1)]
    [InlineData(2, 0.2)]
    public async Task CreateHttpClientContext_Policy_Test(int retryCount, double timeoutSeconds)
    {
        var expectedTime = TimeSpan.FromSeconds(timeoutSeconds).Multiply(retryCount + 1);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var http = HttpClientService.Create(m =>
        {
            m.ConnectTimeout = TimeSpan.FromMinutes(1);
            m.SleepDurationProvider = m => TimeSpan.Zero;
            m.RetryCount = retryCount;
        });
        var response = await HttpRequest.Get("https://www.google.com:444/")
            .ReadHeadersTimeout(timeout)
            .SendAsync(http);

        Assert.True(response.HasError);
        output.WriteLine(response.Exception.ToString());

        Assert.IsType<TaskCanceledException>(response.Exception);
        AssertEx.Equal(expectedTime, response.Elapsed, TimeSpan.FromSeconds(0.2));
    }
}