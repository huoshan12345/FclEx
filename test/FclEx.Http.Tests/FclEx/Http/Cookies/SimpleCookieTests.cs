namespace FclEx.Http.Cookies;

public class SimpleCookieTests
{
    [Fact]
    public void DefaultConstructor_UsesEmptyNameValueDomainAndRootPath()
    {
        var cookie = new SimpleCookie();

        Assert.Equal("", cookie.Name);
        Assert.Equal("", cookie.Value);
        Assert.Equal("", cookie.Domain);
        Assert.Equal("/", cookie.Path);
    }

    [Fact]
    public void Constructor_WhenOptionalValuesAreNull_UsesEmptyValueDomainAndRootPath()
    {
        var cookie = new SimpleCookie("sid", null, null, null);

        Assert.Equal("sid", cookie.Name);
        Assert.Equal("", cookie.Value);
        Assert.Equal("", cookie.Domain);
        Assert.Equal("/", cookie.Path);
    }

    [Fact]
    public void Constructor_WhenNameIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new SimpleCookie(null!, "abc", "/", "example.com"));

        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void ToSimpleCookie_PreservesCookiePath()
    {
        var cookie = new Cookie("sid", "abc", "/account", "example.com");

        var simpleCookie = cookie.ToSimpleCookie();

        Assert.Equal("sid", simpleCookie.Name);
        Assert.Equal("abc", simpleCookie.Value);
        Assert.Equal("/account", simpleCookie.Path);
        Assert.Equal("example.com", simpleCookie.Domain);
    }

    [Fact]
    public void ToCookie_UsesStoredPathAndDomain()
    {
        var simpleCookie = new SimpleCookie("sid", "abc", "/account", "example.com");

        var cookie = simpleCookie.ToCookie();

        Assert.Equal("sid", cookie.Name);
        Assert.Equal("abc", cookie.Value);
        Assert.Equal("/account", cookie.Path);
        Assert.Equal("example.com", cookie.Domain);
    }
}
