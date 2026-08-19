namespace FclEx.Utils;

public class UriCreatorTests
{
    [Fact]
    public void Constructor_WithoutSchemeHost_ShouldInitializeUriBuilderCorrectly()
    {
        var creator = new UriCreator("", "", -1);
        Assert.Equal("", creator.Scheme);
        Assert.Equal("", creator.Host);
        Assert.Equal(-1, creator.Port);
    }

    [Fact]
    public void Constructor_WithSchemeHostAndPort_ShouldInitializeUriBuilderCorrectly()
    {
        var creator = new UriCreator("http", "example.com", 8080);
        Assert.Equal("http", creator.Scheme);
        Assert.Equal("example.com", creator.Host);
        Assert.Equal(8080, creator.Port);
    }

    [Fact]
    public void Constructor_WithUriString_ShouldInitializeCorrectly()
    {
        var creator = new UriCreator("http://example.com:8080/path?query=value#fragment");
        Assert.Equal("http", creator.Scheme);
        Assert.Equal("example.com", creator.Host);
        Assert.Equal(8080, creator.Port);
        Assert.Equal("/path", creator.Path);
        Assert.Equal("value", creator.Query["query"]);
        Assert.Equal("fragment", creator.Fragment.TrimStart('#'));
    }

    [Fact]
    public void Constructor_WithRelativeUri_ShouldInitializeCorrectly()
    {
        var creator = new UriCreator("/path?query=value#fragment");
        Assert.Equal("/path", creator.Path);
        Assert.Equal("value", creator.Query["query"]);
        Assert.Equal("fragment", creator.Fragment.TrimStart('#'));
    }

    [Fact]
    public void Build_WithFullUri_ShouldReturnExpectedUri()
    {
        var creator = new UriCreator("http", "example.com", 8080, "/path")
        {
            Query =
            {
                ["query"] = "value",
            },
            Fragment = "#fragment",
        };

        var result = creator.Build();

        Assert.Equal("http://example.com:8080/path?query=value#fragment", result.ToString());
    }

    [Fact]
    public void Build_WithRelativeUri_ShouldReturnExpectedRelativeUri()
    {
        var creator = new UriCreator("/path")
        {
            Query =
            {
                ["query"] = "value",
            },
            Fragment = "#fragment",
        };

        var result = creator.Build();

        Assert.Equal("/path?query=value#fragment", result.ToString());
    }

    [Fact]
    public void SplitUri_WithQueryAndFragment_ShouldReturnCorrectComponents()
    {
        var (path, query, fragment) = UriCreator.SplitUri("/path?query=value#fragment");

        Assert.Equal("/path", path);
        Assert.Equal("query=value", query);
        Assert.Equal("fragment", fragment);
    }

    [Fact]
    public void SplitUri_WithOnlyPath_ShouldReturnPathOnly()
    {
        var (path, query, fragment) = UriCreator.SplitUri("/path");

        Assert.Equal("/path", path);
        Assert.Equal("", query);
        Assert.Equal("", fragment);
    }

    [Fact]
    public void SplitUri_WithPathAndQuery_ShouldReturnPathAndQuery()
    {
        var (path, query, fragment) = UriCreator.SplitUri("/path?query=value");

        Assert.Equal("/path", path);
        Assert.Equal("query=value", query);
        Assert.Equal("", fragment);
    }

    [Fact]
    public void SplitUri_WithQuestionMarkInFragment_ShouldKeepItInFragment()
    {
        var (path, query, fragment) = UriCreator.SplitUri("/path#fragment?query=value");

        Assert.Equal("/path", path);
        Assert.Equal("", query);
        Assert.Equal("fragment?query=value", fragment);
    }

    [Fact]
    public void SplitUri_WithQueryAndQuestionMarkInFragment_ShouldSplitAtDelimitersBeforeFragmentOnly()
    {
        var (path, query, fragment) = UriCreator.SplitUri("/path?query=value#fragment?not-a-query");

        Assert.Equal("/path", path);
        Assert.Equal("query=value", query);
        Assert.Equal("fragment?not-a-query", fragment);
    }

    [Fact]
    public void Host_SetWithPort_ShouldUpdateHostAndPort()
    {
        var creator = new UriCreator("http", "example.com")
        {
            Host = "example.com:8080",
        };

        Assert.Equal("example.com", creator.Host);
        Assert.Equal(8080, creator.Port);
    }

    [Fact]
    public void Host_SetWithIpv4AndPort_ShouldUpdateHostAndPort()
    {
        var creator = new UriCreator("http", "example.com")
        {
            Host = "192.168.1.1:8080",
        };

        Assert.Equal("192.168.1.1", creator.Host);
        Assert.Equal(8080, creator.Port);
    }

    [Fact]
    public void Host_SetWithIpv4_ShouldUpdateHost()
    {
        var creator = new UriCreator("http", "example.com", -1)
        {
            Host = "192.168.1.1",
        };

        Assert.Equal("192.168.1.1", creator.Host);
        Assert.Equal(-1, creator.Port);
    }

    [Fact]
    public void Host_SetWithIpv6AndPort_ShouldUpdateHostAndPort()
    {
        var creator = new UriCreator("http", "example.com")
        {
            Host = "[2001:db8::1]:8080",
        };

        Assert.Equal("[2001:db8::1]", creator.Host);
        Assert.Equal(8080, creator.Port);
    }

    [Fact]
    public void Host_SetWithIpv6_ShouldUpdateHost()
    {
        var creator = new UriCreator("http", "example.com")
        {
            Host = "[2001:db8::1]",
        };

        Assert.Equal("[2001:db8::1]", creator.Host);
        Assert.Equal(-1, creator.Port);
    }

    [Fact]
    public void ToString_ShouldReturnBuiltUriString()
    {
        var creator = new UriCreator("http", "example.com", 8080, "/path")
        {
            Query =
            {
                ["query"] = "value",
            },
            Fragment = "#fragment",
        };

        Assert.Equal("http://example.com:8080/path?query=value#fragment", creator.ToString());
    }
}
