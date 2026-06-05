namespace FclEx.Http.Utils;

public class WebProxyEqualityComparerTests
{
    public static readonly IEqualityComparer<WebProxy> Comparer = WebProxyEqualityComparer.Instance;

    [Fact]
    public void Equals_Empty_Test()
    {
        Assert.True(Comparer.Equals(WebProxyHelper.Empty, new WebProxy()));
    }

    [Fact]
    public void GetHashCode_Empty_Test()
    {
        Assert.Equal(
            Comparer.GetHashCode(WebProxyHelper.Empty), 
            Comparer.GetHashCode(new WebProxy()));
    }

    [Fact]
    public void Equals_SameUri_Test()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var proxy1 = WebProxyHelper.Create(uri);
        var proxy2 = WebProxyHelper.Create(uri);

        Assert.True(Comparer.Equals(proxy1, proxy2));
    }

    [Fact]
    public void GetHashCode_SameUri_Test()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        var proxy1 = WebProxyHelper.Create(uri);
        var proxy2 = WebProxyHelper.Create(uri);

        Assert.Equal(
            Comparer.GetHashCode(proxy1),
            Comparer.GetHashCode(proxy2));
    }

    [Fact]
    public void InterfaceComparer_UsesWebProxyValueEquality()
    {
        var uri = new Uri("http://127.0.0.1:8888");
        IWebProxy proxy1 = WebProxyHelper.Create(uri)!;
        IWebProxy proxy2 = WebProxyHelper.Create(uri)!;

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(proxy1, proxy2));
        Assert.Equal(
            WebProxyInterfaceEqualityComparer.Instance.GetHashCode(proxy1),
            WebProxyInterfaceEqualityComparer.Instance.GetHashCode(proxy2));
    }

    [Fact]
    public void Equals_WhenNetworkCredentialsHaveSameValues_UsesCredentialValueEquality()
    {
        var proxy1 = WebProxyHelper.Create("http://user:pass@127.0.0.1:8888");
        var proxy2 = WebProxyHelper.Create(
            new Uri("http://127.0.0.1:8888"),
            credentials: new NetworkCredential("user", "pass"));

        Assert.True(Comparer.Equals(proxy1, proxy2));
        Assert.Equal(Comparer.GetHashCode(proxy1), Comparer.GetHashCode(proxy2));
    }
}
