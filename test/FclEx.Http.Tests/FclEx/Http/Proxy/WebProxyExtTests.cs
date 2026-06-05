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
}
