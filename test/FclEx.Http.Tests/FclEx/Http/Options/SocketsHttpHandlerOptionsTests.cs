namespace FclEx.Http.Options;

public class SocketsHttpHandlerOptionsTests
{
    [Fact]
    public void Constructor_UsesExpectedDefaults()
    {
        var options = new SocketsHttpHandlerOptions();

        Assert.Equal(TimeSpan.FromSeconds(5), options.ConnectTimeout);
        Assert.Equal(IPVersionPolicy.PreferIPv4, options.IPVersionPolicy);
        Assert.True(options.AllowAutoRedirect);
#if NET5_0_OR_GREATER
        Assert.Equal(DecompressionMethods.All, options.AutomaticDecompression);
#else
        Assert.Equal(DecompressionMethods.GZip, options.AutomaticDecompression);
#endif
        Assert.Null(options.Proxy);
        Assert.False(options.EnableMultipleHttp2Connections);
        Assert.Equal(TimeSpan.FromMinutes(1), options.PooledConnectionLifetime);
        Assert.Equal(TimeSpan.FromMinutes(2), options.PooledConnectionIdleTimeout);
        Assert.False(options.DisableServerCertificateValidation);
    }
}
