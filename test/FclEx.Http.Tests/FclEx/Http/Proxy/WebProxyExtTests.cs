namespace FclEx.Http.Proxy;

public class WebProxyExtTests
{
    [Fact]
    public void WebProxyHelper_IsStaticUtilityClass()
    {
        var type = typeof(WebProxyHelper);

        Assert.True(type.IsAbstract);
        Assert.True(type.IsSealed);
    }

    [Theory]
    [InlineData("userName", "password")]
    [InlineData("user@Name", "pass@word")]
    public void Create_WithAuthUri(string userName, string password)
    {
        var uriBuilder = new UriBuilder("http://192.168.1.221:8888")
        {
            UserName = userName.UriEscape(),
            Password = password.UriEscape()
        };

        var proxy = WebProxyHelper.Create(uriBuilder.Uri);
        var auth = proxy.Credentials.CastTo<NetworkCredential>()!;
        Assert.Equal(userName, auth.UserName);
        Assert.Equal(password, auth.Password);
    }

    [Fact]
    public void Create_WithAuthUri_RemovesUserInfoFromProxyAddress()
    {
        var proxy = WebProxyHelper.Create("http://user:pass@127.0.0.1:8888/proxy");

        Assert.Equal(new Uri("http://127.0.0.1:8888/proxy"), proxy.Address);
    }

    [Fact]
    public void Create_WhenExplicitCredentialsAreProvided_DoesNotUseCredentialsFromUri()
    {
        var credentials = new NetworkCredential("explicit-user", "explicit-pass");

        var proxy = WebProxyHelper.Create("http://uri-user:uri-pass@127.0.0.1:8888", credentials: credentials);

        Assert.Same(credentials, proxy.Credentials);
        Assert.Equal(new Uri("http://uri-user:uri-pass@127.0.0.1:8888/"), proxy.Address);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WhenAddressIsNullOrEmpty_ReturnsProxyWithoutAddress(string? address)
    {
        var proxy = WebProxyHelper.Create(address);

        Assert.Null(proxy.Address);
    }

    [Fact]
    public void Create_CopiesBypassOptions()
    {
        string[] bypassList = [@".*\.local", @"127\.0\.0\.1"];

        var proxy = WebProxyHelper.Create(
            "http://127.0.0.1:8888",
            bypassOnLocal: true,
            bypassList: bypassList);

        Assert.True(proxy.BypassProxyOnLocal);
        Assert.Equal(bypassList, proxy.BypassList);
    }
}
