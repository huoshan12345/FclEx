namespace FclEx;

public class RegexesExtensionsTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("prefix user@example.com suffix", true)]
    [InlineData("user@example", false)]
    [InlineData("user.name@example.com", true)]
    [InlineData("user@example.toolong", true)]
    public void EmailCheck_MatchesExistingPatternBehavior(string value, bool expected)
    {
        Assert.Equal(expected, Regexes.EmailCheck.IsMatch(value));
    }

    [Theory]
    [InlineData("""<meta charset="utf-8">""", "utf-8")]
    [InlineData("""<meta charset='gb2312'>""", "gb2312")]
    [InlineData("""<meta charset=utf-8>""", "utf-8")]
    [InlineData("""<meta http-equiv="Content-Type" content="text/html; charset=utf-8">""", "utf-8")]
    [InlineData("""<meta content='text/html; charset=gb2312' http-equiv='Content-Type'>""", "gb2312")]
    public void CharSet_ExtractsCharsetFromSupportedMetaForms(string html, string expected)
    {
        var match = Regexes.CharSet
            .Select(m => m.Match(html))
            .First(m => m.Success);

        Assert.Equal(expected, match.Groups["charset"].Value);
    }

    [Fact]
    public void CharSet_WhenNoMetaCharsetExists_DoesNotMatch()
    {
        Assert.All(Regexes.CharSet, regex => Assert.False(regex.IsMatch("<html><head></head></html>")));
    }
}
