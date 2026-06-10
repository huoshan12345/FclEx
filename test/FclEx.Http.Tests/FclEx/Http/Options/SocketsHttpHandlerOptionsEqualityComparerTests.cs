namespace FclEx.Http.Options;

public class SocketsHttpHandlerOptionsEqualityComparerTests
{
    private static readonly SocketsHttpHandlerOptionsEqualityComparer Comparer = SocketsHttpHandlerOptionsEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenOptionsHaveSameValues_ReturnsTrueAndHashCodesMatch()
    {
        var options1 = CreateOptions();
        var options2 = CreateOptions();

        Assert.True(Comparer.Equals(options1, options2));
        Assert.Equal(Comparer.GetHashCode(options1), Comparer.GetHashCode(options2));
    }

    [Fact]
    public void Equals_WhenProxyValuesMatch_UsesProxyValueEquality()
    {
        var options1 = new SocketsHttpHandlerOptions
        {
            Proxy = WebProxyHelper.Create("http://user:pass@127.0.0.1:8888")
        };
        var options2 = new SocketsHttpHandlerOptions
        {
            Proxy = WebProxyHelper.Create(
                new Uri("http://127.0.0.1:8888"),
                credentials: new NetworkCredential("user", "pass"))
        };

        Assert.True(Comparer.Equals(options1, options2));
        Assert.Equal(Comparer.GetHashCode(options1), Comparer.GetHashCode(options2));
    }

    [Theory]
    [MemberData(nameof(DifferentOptions))]
    public void Equals_WhenAnyComparedOptionDiffers_ReturnsFalse(SocketsHttpHandlerOptions left, SocketsHttpHandlerOptions right)
    {
        Assert.False(Comparer.Equals(left, right));
    }

    public static IEnumerable<object[]> DifferentOptions()
    {
        yield return Diff(m => m.ConnectTimeout = TimeSpan.FromSeconds(10));
        yield return Diff(m => m.IPVersionPolicy = IPVersionPolicy.OnlyIPv6);
        yield return Diff(m => m.AllowAutoRedirect = false);
        yield return Diff(m => m.AutomaticDecompression = DecompressionMethods.None);
        yield return Diff(m => m.EnableMultipleHttp2Connections = true);
        yield return Diff(m => m.PooledConnectionLifetime = TimeSpan.FromMinutes(5));
        yield return Diff(m => m.PooledConnectionIdleTimeout = TimeSpan.FromMinutes(6));
        yield return Diff(m => m.DisableServerCertificateValidation = true);
        yield return Diff(m => m.Proxy = WebProxyHelper.Create("http://127.0.0.1:9999"));
    }

    private static object[] Diff(Action<SocketsHttpHandlerOptions> mutate)
    {
        var left = CreateOptions();
        var right = CreateOptions();
        mutate(right);
        return [left, right];
    }

    private static SocketsHttpHandlerOptions CreateOptions()
    {
        return new SocketsHttpHandlerOptions
        {
            ConnectTimeout = TimeSpan.FromSeconds(3),
            IPVersionPolicy = IPVersionPolicy.PreferIPv4,
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            EnableMultipleHttp2Connections = false,
            PooledConnectionLifetime = TimeSpan.FromMinutes(1),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            DisableServerCertificateValidation = false,
            Proxy = WebProxyHelper.Create("http://user:pass@127.0.0.1:8888")
        };
    }
}
