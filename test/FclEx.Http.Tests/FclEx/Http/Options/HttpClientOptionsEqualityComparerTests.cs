namespace FclEx.Http.Options;

public class HttpClientOptionsEqualityComparerTests
{
    private static readonly HttpClientOptionsEqualityComparer Comparer = HttpClientOptionsEqualityComparer.Instance;

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
}
