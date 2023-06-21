namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    private readonly ITestOutputHelper _output;

    public HttpClientServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://localhost:1080")]
    public void Constructor_Test(string proxy)
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
            AssertExt.False(res.HasError, () => res.Exception!.ToString());
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_Test(bool useCookie)
    {
        var uri = new Uri("https://www.instagram.com/");
        var cookies = GlobalConstants.SimpleCookies;
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, uri);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_NullUri_Test(bool useCookie)
    {
        var cookies = GlobalConstants.SimpleCookies;
        using var service = HttpClientService.Create(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAllCookies_Test(bool useCookie)
    {
        var cookies = GlobalConstants.SimpleCookies.Select(m => m.ToCookie()).ToDictionary(m => m.Name);
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
        var fac1 = HttpClientService.GetFactory(HttpClientOptions.Default);
        var fac2 = HttpClientService.GetFactory(HttpClientOptions.Default);
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
    }

    private static void CheckHttpClient(HttpClient client, Uri? baseAddress, IWebProxy? proxy)
    {
        Assert.Equal(baseAddress, client.BaseAddress);
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
        var fac1 = HttpClientService.GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        var fac2 = HttpClientService.GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        Assert.Equal(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckHttpClient(fac1.CreateClient(), null, WebProxyHelper.Create(uri));
        CheckHttpClient(fac2.CreateClient(), null, WebProxyHelper.Create(uri));
    }

    [Fact]
    public void SetProxy_Test()
    {
        var http = HttpClientService.Create();
        {
            var client = http.CreateClient();
            Assert.Null(http.Proxy);
            CheckHttpClient(client, null, null);
        }
        {
            var proxy = WebProxyHelper.Create("http://127.0.0.1:8888");
            http.Proxy = proxy;
            var client = http.CreateClient();
            Assert.Equal(proxy, http.Proxy);
            CheckHttpClient(client, null, proxy);
        }
    }

    [Fact]
    public void GetFactory_Proxy_NotSame()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var fac1 = HttpClientService.GetFactory(new() { Proxy = WebProxyHelper.Create(uri) });
        var fac2 = HttpClientService.GetFactory(new() { Proxy = null });
        Assert.NotEqual(fac1, fac2, ReferenceEqualityComparer.Instance);
        CheckHttpClient(fac1.CreateClient(), null, WebProxyHelper.Create(uri));
        CheckHttpClient(fac2.CreateClient(), null, null);
    }
}