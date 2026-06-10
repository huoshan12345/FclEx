namespace FclEx.Http.Utils;

public class UriOriginEqualityComparerTests
{
    private static readonly UriOriginEqualityComparer Comparer = UriOriginEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenOnlyPathQueryUserInfoAndCaseDiffer_ReturnsTrueAndHashCodesMatch()
    {
        var uri1 = new Uri("HTTPS://user:pass@EXAMPLE.com:443/a/b?x=1#fragment");
        var uri2 = new Uri("https://example.COM:443/other/path?y=2");

        Assert.True(Comparer.Equals(uri1, uri2));
        Assert.Equal(Comparer.GetHashCode(uri1), Comparer.GetHashCode(uri2));
    }

    [Theory]
    [InlineData("http://example.com:80", "https://example.com:443")]
    [InlineData("https://example.com:443", "https://example.com:8443")]
    [InlineData("https://example.com:443", "https://other.example.com:443")]
    public void Equals_WhenOriginDiffers_ReturnsFalse(string left, string right)
    {
        Assert.False(Comparer.Equals(new Uri(left), new Uri(right)));
    }

    [Fact]
    public void Equals_WhenBothAreNull_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_WhenOnlyOneSideIsNull_ReturnsFalse()
    {
        Assert.False(Comparer.Equals(new Uri("https://example.com"), null));
        Assert.False(Comparer.Equals(null, new Uri("https://example.com")));
    }
}
