using Xunit.Abstractions;

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
        var http = new HttpClientService(proxy: WebProxyHelper.Create(proxy));
        Assert.Equal(WebProxyHelper.Create(proxy).CastTo<WebProxy>().Address, http.WebProxy.CastTo<WebProxy>()!.Address);
    }

    [Fact]
    public async Task SendAsync_Success()
    {
        using var service = new HttpClientService(false);
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
        using var service = new HttpClientService(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, uri);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCookie_NullUri_Test(bool useCookie)
    {
        var cookies = GlobalConstants.SimpleCookies;
        using var service = new HttpClientService(useCookie);
        foreach (var cookie in cookies.Select(m => m.ToCookie()))
            service.AddCookie(cookie, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetAllCookies_Test(bool useCookie)
    {
        var cookies = GlobalConstants.SimpleCookies.Select(m => m.ToCookie()).ToDictionary(m => m.Name);
        using var service = new HttpClientService(useCookie);
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
}