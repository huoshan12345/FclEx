namespace FclEx.Utils;

public class UriCreatorExtensionsTests
{
    [Fact]
    public void Scheme_ShouldUpdateScheme()
    {
        var creator = new UriCreator("http", "example.com");
        creator.Scheme("https");

        Assert.Equal("https", creator.Scheme);
    }

    [Fact]
    public void Host_ShouldUpdateHost()
    {
        var creator = new UriCreator("http", "example.com");
        creator.Host("new-example.com");

        Assert.Equal("new-example.com", creator.Host);
    }

    [Fact]
    public void Port_ShouldUpdatePort()
    {
        var creator = new UriCreator("http", "example.com");
        creator.Port(8080);

        Assert.Equal(8080, creator.Port);
    }

    [Fact]
    public void UserName_ShouldUpdateUserName()
    {
        var creator = new UriCreator("http", "example.com");
        creator.UserName("user123");

        Assert.Equal("user123", creator.UserName);
    }

    [Fact]
    public void Path_ShouldUpdatePath()
    {
        var creator = new UriCreator("http", "example.com");
        creator.Path("/new-path");

        Assert.Equal("/new-path", creator.Path);
    }

    [Fact]
    public void Fragment_ShouldUpdateFragment()
    {
        var creator = new UriCreator("http", "example.com");
        creator.Fragment("section1");

        Assert.Equal("#section1", creator.Fragment);
    }

    [Fact]
    public void AddQueryParam_ShouldAddQueryParameter()
    {
        var creator = new UriCreator("http", "example.com");
        creator.AddQueryParam("key", "value");

        Assert.Equal("value", creator.Query["key"]);
    }

    [Fact]
    public void AddQueryParam_ShouldHandleNullValue()
    {
        var creator = new UriCreator("http", "example.com");
        creator.AddQueryParam("key", null);

        Assert.Equal(string.Empty, creator.Query["key"]);
    }

    [Fact]
    public void MultipleExtensionMethods_ShouldChainCorrectly()
    {
        var creator = new UriCreator("http", "example.com")
            .Scheme("https")
            .Host("secure.com")
            .Port(443)
            .UserName("admin")
            .Path("/dashboard")
            .Fragment("settings")
            .AddQueryParam("theme", "dark");

        Assert.Equal("https", creator.Scheme);
        Assert.Equal("secure.com", creator.Host);
        Assert.Equal(443, creator.Port);
        Assert.Equal("admin", creator.UserName);
        Assert.Equal("/dashboard", creator.Path);
        Assert.Equal("#settings", creator.Fragment);
        Assert.Equal("dark", creator.Query["theme"]);
    }
}