namespace FclEx.Http.Utils;

public class WebProxyEqualityComparerTests
{
    public static readonly IEqualityComparer<WebProxy> Comparer = WebProxyEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenBothProxiesAreEmpty_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(WebProxy.Empty, new WebProxy()));
    }

    [Fact]
    public void GetHashCode_WhenBothProxiesAreEmpty_ReturnsSameHashCode()
    {
        Assert.Equal(
            Comparer.GetHashCode(WebProxy.Empty), 
            Comparer.GetHashCode(new WebProxy()));
    }

    [Fact]
    public void Equals_WhenProxyAddressesMatch_ReturnsTrue()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var proxy1 = WebProxy.Create(uri);
        var proxy2 = WebProxy.Create(uri);

        Assert.True(Comparer.Equals(proxy1, proxy2));
    }

    [Fact]
    public void GetHashCode_WhenProxyAddressesMatch_ReturnsSameHashCode()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var proxy1 = WebProxy.Create(uri);
        var proxy2 = WebProxy.Create(uri);

        Assert.Equal(
            Comparer.GetHashCode(proxy1),
            Comparer.GetHashCode(proxy2));
    }

    [Fact]
    public void InterfaceComparer_UsesWebProxyValueEquality()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        IWebProxy proxy1 = WebProxy.Create(uri)!;
        IWebProxy proxy2 = WebProxy.Create(uri)!;

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(proxy1, proxy2));
        Assert.Equal(
            WebProxyInterfaceEqualityComparer.Instance.GetHashCode(proxy1),
            WebProxyInterfaceEqualityComparer.Instance.GetHashCode(proxy2));
    }

    [Fact]
    public void Equals_WhenNetworkCredentialsHaveSameValues_UsesCredentialValueEquality()
    {
        var proxy1 = WebProxy.Create("http://user:pass@127.0.0.1:8888");
        var proxy2 = WebProxy.Create(
            new Uri("http://127.0.0.1:8888"),
            credentials: new NetworkCredential("user", "pass"));

        Assert.True(Comparer.Equals(proxy1, proxy2));
        Assert.Equal(Comparer.GetHashCode(proxy1), Comparer.GetHashCode(proxy2));
    }

    [Fact]
    public void Equals_WhenBypassListDiffersOnlyByCase_TreatsBypassListAsEqual()
    {
        var proxy1 = WebProxy.Create(
            new Uri("http://127.0.0.1:8888"),
            bypassList: ["LOCALHOST", "EXAMPLE\\.COM"]);
        var proxy2 = WebProxy.Create(
            new Uri("http://127.0.0.1:8888"),
            bypassList: ["localhost", "example\\.com"]);

        Assert.True(Comparer.Equals(proxy1, proxy2));
    }

    [Fact]
    public void GetHashCode_WhenBypassListDiffersOnlyByCase_ReturnsSameHashCode()
    {
        var proxy1 = WebProxy.Create(
            new Uri("http://127.0.0.1:8888"),
            bypassList: ["LOCALHOST", "EXAMPLE\\.COM"]);
        var proxy2 = WebProxy.Create(
            new Uri("http://127.0.0.1:8888"),
            bypassList: ["localhost", "example\\.com"]);

        Assert.Equal(Comparer.GetHashCode(proxy1), Comparer.GetHashCode(proxy2));
    }

    [Fact]
    public void Equals_WhenBypassProxyOnLocalDiffers_ReturnsFalse()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var proxy1 = WebProxy.Create(uri, bypassOnLocal: true);
        var proxy2 = WebProxy.Create(uri, bypassOnLocal: false);

        Assert.False(Comparer.Equals(proxy1, proxy2));
    }

    [Fact]
    public void InterfaceComparer_WhenCustomProxyInstancesDiffer_ReturnsFalse()
    {
        IWebProxy proxy1 = new CustomProxy();
        IWebProxy proxy2 = new CustomProxy();

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(proxy1, proxy1));
        Assert.False(WebProxyInterfaceEqualityComparer.Instance.Equals(proxy1, proxy2));
    }

    private sealed class CustomProxy : IWebProxy
    {
        public ICredentials? Credentials { get; set; }

        public Uri? GetProxy(Uri destination) => destination;

        public bool IsBypassed(Uri host) => false;
    }
}
