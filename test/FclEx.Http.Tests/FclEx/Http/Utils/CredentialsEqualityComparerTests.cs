namespace FclEx.Http.Utils;

public class CredentialsEqualityComparerTests
{
    private static readonly CredentialsEqualityComparer Comparer = CredentialsEqualityComparer.Instance;

    [Fact]
    public void Equals_WhenNetworkCredentialsHaveSameValues_ReturnsTrueAndHashCodesMatch()
    {
        ICredentials credential1 = new NetworkCredential("user", "password", "domain");
        ICredentials credential2 = new NetworkCredential("user", "password", "domain");

        Assert.True(Comparer.Equals(credential1, credential2));
        Assert.Equal(Comparer.GetHashCode(credential1), Comparer.GetHashCode(credential2));
    }

    [Fact]
    public void Equals_WhenNetworkCredentialValueDiffers_ReturnsFalse()
    {
        ICredentials credential1 = new NetworkCredential("user", "password");
        ICredentials credential2 = new NetworkCredential("user", "other");

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

    [Fact]
    public void Equals_WhenCustomCredentialInstancesDiffer_ReturnsFalse()
    {
        ICredentials credential1 = new CustomCredentials();
        ICredentials credential2 = new CustomCredentials();

        Assert.True(Comparer.Equals(credential1, credential1));
        Assert.False(Comparer.Equals(credential1, credential2));
    }

    private sealed class CustomCredentials : ICredentials
    {
        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            return new NetworkCredential("user", "password");
        }
    }
}
