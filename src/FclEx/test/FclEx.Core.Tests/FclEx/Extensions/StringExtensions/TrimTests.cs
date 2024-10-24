#nullable enable
namespace FclEx.Extensions.StringExtensions;

public record TrimCase(string? Source, string? Trim, string? Result);

public class TrimTests
{
    public static readonly IEnumerable<object?[]> TrimStartCases = new TrimCase[]
    {
        new(null, null, null),
        new(null, "", null),
        new("", null, ""),
        new("aa_xx", "aa", "_xx"),
        new("aaaa_xx", "aa", "_xx"),
        new("aaaaa_xx", "aa", "a_xx"),
        new("aaaaaa_xx", "aa", "_xx"),
        new("aa_xx", "_", "aa_xx"),
        new("aa_xx", "", "aa_xx"),
        new("aa_xx", "xx", "aa_xx"),
    }.Select(m => new object?[] { m.Source, m.Trim, m.Result });

    [Theory]
    [MemberData(nameof(TrimStartCases))]
    public void TrimStart_Test(string source, string trim, string result)
    {
        var actual = source.TrimStart(trim);
        Assert.Equal(result, actual);
    }

    public static readonly IEnumerable<object?[]> TrimEndCases = new TrimCase[]
    {
        new(null, null, null),
        new(null, "", null),
        new("", null, ""),
        new("aa_xx", "xx", "aa_"),
        new("aa_xxx", "xx", "aa_x"),
        new("aa_xxxx", "xx", "aa_"),
        new("aa_xxxxxx", "xx", "aa_"),
        new("aa_xx", "_", "aa_xx"),
        new("aa_xx", "", "aa_xx"),
        new("aa_xx", "aa", "aa_xx"),
    }.Select(m => new object?[] { m.Source, m.Trim, m.Result });

    [Theory]
    [MemberData(nameof(TrimEndCases))]
    public void TrimEnd_Test(string source, string trim, string result)
    {
        var actual = source.TrimEnd(trim);
        Assert.Equal(result, actual);
    }

    private const string Base64ImgPrefix = "data:image/png;base64";
    private const string Base64ImgContent = "iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==";
    private const string Base64Img = Base64ImgPrefix + "," + Base64ImgContent;

    [Fact]
    public void TrimStart_Contains_Test()
    {
        var source = "Bearer token";
        var result = source.TrimStart("Bearer ");
        Assert.Equal("token", result);
    }

    [Fact]
    public void TrimStart_DoesNotContain_Test()
    {
        var source = "Basic token";
        var result = source.TrimStart("Bearer ");
        Assert.Equal("Basic token", result);
    }

    [Fact]
    public void TrimStart_Null_Source_Test()
    {
        string? source = null;
        var result = source.TrimStart("Bearer ");
        Assert.Null(result);
    }

    [Fact]
    public void TrimStart_Null_TrimString_Test()
    {
        var source = "Basic token";
        var result = source.TrimStart(null);
        Assert.Equal(source, result);
    }

    [Fact]
    public void SkipUntil_DoesNotContainsSeparator()
    {
        const string text = "data";
        var result = text.SkipUntil(",");
        Assert.Equal(text, result);
    }

    [Fact]
    public void SkipUntil_SkipSeparator()
    {
        const string text = "data.ext.ext2";
        var newText = text.SkipUntil(".");
        Assert.Equal("ext.ext2", newText);
    }

    [Fact]
    public void SkipUntil_DoesNotSkipSeparator()
    {
        const string text = "data.ext.ext2";
        var newText = text.SkipUntil(".", skipSeparator: false);
        Assert.Equal(".ext.ext2", newText);
    }

    [Fact]
    public void SkipUntil_SkipSeparator_UntilLast()
    {
        const string text = "data.ext.ext2";
        var newText = text.SkipUntil(".", untilLast: true);
        Assert.Equal("ext2", newText);
    }

    [Fact]
    public void SkipUntil_DoesNotSkipSeparator_UntilLast()
    {
        const string text = "data.ext.ext2";
        var newText = text.SkipUntil(".", skipSeparator: false, untilLast: true);
        Assert.Equal(".ext2", newText);
    }

    [Fact]
    public void TakeUntil_IncludeSeparator()
    {
        const string text = "data.ext.ext2";
        var newText = text.TakeUntil(".", includeSeparator: true);
        Assert.Equal("data.", newText);
    }

    [Fact]
    public void TakeUntil_IncludeSeparator_UntilLast()
    {
        const string text = "data.ext.ext2";
        var newText = text.TakeUntil(".", includeSeparator: true, untilLast: true);
        Assert.Equal("data.ext.", newText);
    }

    [Fact]
    public void TakeUntil_DoesNotIncludeSeparator()
    {
        const string text = "data.ext.ext2";
        var newText = text.TakeUntil(".", includeSeparator: false);
        Assert.Equal("data", newText);
    }

    [Fact]
    public void TakeUntil_DoesNotIncludeSeparator_UntilLast()
    {
        const string text = "data.ext.ext2";
        var newText = text.TakeUntil(".", includeSeparator: false, untilLast: true);
        Assert.Equal("data.ext", newText);
    }
}