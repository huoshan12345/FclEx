namespace FclEx.Web.Testing;

public class LoginOptionsTests
{
    [Fact]
    public void Constructor_SetsAllValues()
    {
        var proxy = WebProxy.Create("http://127.0.0.1:8888");

        var options = new LoginOptions(
            Login: true,
            FakeLogin: false,
            UseCache: true,
            ReadCookie: false,
            Proxy: proxy);

        Assert.True(options.Login);
        Assert.False(options.FakeLogin);
        Assert.True(options.UseCache);
        Assert.False(options.ReadCookie);
        Assert.Same(proxy, options.Proxy);
    }
}
