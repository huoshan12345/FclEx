namespace FclEx.Http.Utils;

public class NetworkCredentialEqualityComparerTests
{
    private static readonly NetworkCredentialEqualityComparer Comparer = NetworkCredentialEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenUserNamePasswordAndDomainMatch_ReturnsTrueAndHashCodesMatch()
    {
        var credential1 = new NetworkCredential("user", "password", "domain");
        var credential2 = new NetworkCredential("user", "password", "domain");

        Assert.True(Comparer.Equals(credential1, credential2));
        Assert.Equal(Comparer.GetHashCode(credential1), Comparer.GetHashCode(credential2));
    }

    [Theory]
    [InlineData("other", "password", "domain")]
    [InlineData("user", "other", "domain")]
    [InlineData("user", "password", "other")]
    public void Equals_WhenAnyCredentialPartDiffers_ReturnsFalse(string userName, string password, string domain)
    {
        var credential1 = new NetworkCredential("user", "password", "domain");
        var credential2 = new NetworkCredential(userName, password, domain);

        Assert.False(Comparer.Equals(credential1, credential2));
    }

    [Fact]
    public void Equals_WhenBothAreNull_ReturnsTrue()
    {
        Assert.True(Comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_WhenOnlyOneSideIsNull_ReturnsFalse()
    {
        Assert.False(Comparer.Equals(new NetworkCredential("user", "password"), null));
        Assert.False(Comparer.Equals(null, new NetworkCredential("user", "password")));
    }
}
