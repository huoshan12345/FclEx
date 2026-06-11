namespace FclEx.Http.Options;

public class HttpClientOptionsEqualityComparerTests
{
    private static readonly HttpClientOptionsEqualityComparer Comparer = HttpClientOptionsEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenOptionsHaveSameValues_ReturnsTrueAndHashCodesMatch()
    {
        var options1 = CreateOptions();
        var options2 = CreateOptions();

        Assert.True(Comparer.Equals(options1, options2));
        Assert.Equal(Comparer.GetHashCode(options1), Comparer.GetHashCode(options2));
    }

    [Fact]
    public void GetHashCode_WhenBaseAddressDiffersOnlyByCase_MatchesEquals()
    {
        var options1 = new HttpClientOptions
        {
            BaseAddress = new Uri("https://EXAMPLE.com/Api/")
        };
        var options2 = new HttpClientOptions
        {
            BaseAddress = new Uri("https://example.com/Api/")
        };

        Assert.True(Comparer.Equals(options1, options2));
        Assert.Equal(Comparer.GetHashCode(options1), Comparer.GetHashCode(options2));
    }

    [Fact]
    public void GetHashCode_WhenBaseAddressDiffersOnlyByEscaping_MatchesEquals()
    {
        var options1 = new HttpClientOptions
        {
            BaseAddress = new Uri("https://example.com/a%20b/")
        };
        var options2 = new HttpClientOptions
        {
            BaseAddress = new Uri("https://example.com/a b/")
        };

        Assert.True(Comparer.Equals(options1, options2));
        Assert.Equal(Comparer.GetHashCode(options1), Comparer.GetHashCode(options2));
    }

    [Theory]
    [MemberData(nameof(DifferentOptions))]
    public void Equals_WhenAnyComparedOptionDiffers_ReturnsFalse(HttpClientOptions left, HttpClientOptions right)
    {
        Assert.False(Comparer.Equals(left, right));
    }

    public static IEnumerable<object[]> DifferentOptions()
    {
        yield return Diff(m => m.BaseAddress = new Uri("https://example.net/api/"));
        yield return Diff(m => m.TotalTimeout = TimeSpan.FromSeconds(30));
        yield return Diff(m => m.HandlerOptions.ConnectTimeout = TimeSpan.FromSeconds(10));
        yield return Diff(m => m.RetryPolicyOptions.RetryCount = 5);
#if NET6_0_OR_GREATER
        yield return Diff(m => m.HttpVersion = HttpVersion.Version20);
        yield return Diff(m => m.HttpVersionPolicy = HttpVersionPolicy.RequestVersionExact);
#endif
    }

    private static object[] Diff(Action<HttpClientOptions> mutate)
    {
        var left = CreateOptions();
        var right = CreateOptions();
        mutate(right);
        return [left, right];
    }

    private static HttpClientOptions CreateOptions()
    {
        return new HttpClientOptions
        {
            BaseAddress = new Uri("https://example.com/api/"),
            TotalTimeout = TimeSpan.FromMinutes(2),
            HandlerOptions = new SocketsHttpHandlerOptions
            {
                ConnectTimeout = TimeSpan.FromSeconds(3),
                Proxy = WebProxyHelper.Create("http://127.0.0.1:8888"),
            },
            RetryPolicyOptions = new HttpClientRetryPolicyOptions
            {
                ExecutionTimeout = TimeSpan.FromSeconds(5),
                RetryCount = 2,
                AutoUpdateTotalTimeout = true,
            },
#if NET6_0_OR_GREATER
            HttpVersion = HttpVersion.Version11,
            HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
#endif
        };
    }
}
