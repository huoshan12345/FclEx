namespace FclEx.Extensions.StringExtensions;

public class WrapperTests
{
    public static TheoryData<string, string, string?, bool, string, string, string> StringCases = new()
    {
        ("", "(", ")", false, "()", "", "()"),
        ("()", "(", ")", true, "(())", "", "()"),
        ("((value))", "(", ")", true, "(((value)))", "(value)", "((value))"),
        (" value ", "(", ")", false, "( value )", " value ", "( value )"),
        (" (value) ", "(", ")", false, "( (value) )", " (value) ", "( (value) )"),
        ("value", "\"", null, false, "\"value\"", "value", "\"value\""),
        ("\"value\"", "\"", null, true, "\"\"value\"\"", "value", "\"value\""),
        ("value", "(", null, false, "(value(", "value", "(value("),
        ("/*value*/", "/*", "*/", true, "/*/*value*/*/", "value", "/*value*/"),
        ("/**/", "/*", "*/", true, "/*/**/*/", "", "/**/"),
        ("/*/", "/*", "*/", false, "/*/*/*/", "/*/", "/*/*/*/"),
        ("/*", "/*", "*/", false, "/*/**/", "/*", "/*/**/"),
        ("aaa", "aa", null, false, "aaaaaaa", "aaa", "aaaaaaa"),
        ("aaaa", "aa", null, true, "aaaaaaaa", "", "aaaa"),
        ("aVALUEa", "A", null, false, "AaVALUEaA", "aVALUEa", "AaVALUEaA"),
        ("XvalueX", "X\0", null, false, "X\0XvalueXX\0", "XvalueX", "X\0XvalueXX\0"),
        ("<!--value-->", "<!--", "-->", true, "<!--<!--value-->-->", "value", "<!--value-->"),
        ("BEGINvalueEND", "BEGIN", "END", true, "BEGINBEGINvalueENDEND", "value", "BEGINvalueEND"),
        ("\"a\\\"b\"", "\"", "\"", true, "\"\"a\\\"b\"\"", "a\\\"b", "\"a\\\"b\""),
        ("[中文😀]", "[", "]", true, "[[中文😀]]", "中文😀", "[中文😀]"),
        (" value ", " ", null, true, "  value  ", "value", " value "),
    };

    [Theory]
    [MemberData(nameof(StringCases))]
    public void StringWrappers_FollowBoundaryAndSinglePairContracts(
        string str, string open, string? close, bool matches, string wrapped, string trimmed, string ensured)
    {
        Assert.Equal(matches, str.IsWrappedWith(open, close));
        Assert.Equal(wrapped, str.WrapWith(open, close));
        Assert.Equal(trimmed, str.TrimWrapper(open, close));
        Assert.Equal(ensured, str.EnsureWrappedWith(open, close));
        if (matches)
            Assert.Same(str, str.EnsureWrappedWith(open, close));
        else
            Assert.Same(str, str.TrimWrapper(open, close));
    }

    [Theory]
    [InlineData("", '(', ')', false, "()", "", "()")]
    [InlineData("()", '(', ')', true, "(())", "", "()")]
    [InlineData("(value)", '(', ')', true, "((value))", "value", "(value)")]
    [InlineData("((value))", '(', ')', true, "(((value)))", "(value)", "((value))")]
    [InlineData("(", '(', ')', false, "(()", "(", "(()")]
    [InlineData(")value(", '(', ')', false, "()value()", ")value(", "()value()")]
    [InlineData(" (value) ", '(', ')', false, "( (value) )", " (value) ", "( (value) )")]
    [InlineData("value", '(', null, false, "(value(", "value", "(value(")]
    [InlineData("\"value\"", '"', null, true, "\"\"value\"\"", "value", "\"value\"")]
    [InlineData("\"", '"', null, false, "\"\"\"", "\"", "\"\"\"")]
    [InlineData("\"\"", '"', null, true, "\"\"\"\"", "", "\"\"")]
    [InlineData("\0value\0", '\0', null, true, "\0\0value\0\0", "value", "\0value\0")]
    public void CharWrappers_FollowBoundaryAndSinglePairContracts(
        string str, char open, char? close, bool matches, string wrapped, string trimmed, string ensured)
    {
        Assert.Equal(matches, str.IsWrappedWith(open, close));
        Assert.Equal(wrapped, str.WrapWith(open, close));
        Assert.Equal(trimmed, str.TrimWrapper(open, close));
        Assert.Equal(ensured, str.EnsureWrappedWith(open, close));
        if (matches)
            Assert.Same(str, str.EnsureWrappedWith(open, close));
        else
            Assert.Same(str, str.TrimWrapper(open, close));
    }

    [Fact]
    public void OmittedClose_UsesOpenForEveryOperation()
    {
        Assert.True("/value/".IsWrappedWith("/"));
        Assert.Equal("/value/", "value".WrapWith("/"));
        Assert.Equal("value", "/value/".TrimWrapper("/"));
        Assert.Equal("/value/", "value".EnsureWrappedWith("/"));
        Assert.True("/value/".IsWrappedWith('/'));
        Assert.Equal("/value/", "value".WrapWith('/'));
        Assert.Equal("value", "/value/".TrimWrapper('/'));
        Assert.Equal("/value/", "value".EnsureWrappedWith('/'));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void StringWrappers_RejectNullSourceAndNullOrEmptyDelimiters(int operation)
    {
        var strException = Assert.Throws<ArgumentNullException>(() => ApplyString(operation, null!, "(", ")"));
        Assert.Equal("str", strException.ParamName);
        var openException = Assert.Throws<ArgumentNullException>(() => ApplyString(operation, "value", null!, ")"));
        Assert.Equal("open", openException.ParamName);
        Assert.Throws<ArgumentException>(() => ApplyString(operation, "value", "", ")"));
        Assert.Throws<ArgumentException>(() => ApplyString(operation, "value", "(", ""));
        Assert.Throws<ArgumentException>(() => ApplyString(operation, "value", "", null));
        // Validation must still occur for short input or for an unmatched opening delimiter.
        Assert.Throws<ArgumentException>(() => ApplyString(operation, "", "(", ""));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CharWrappers_RejectNullSource(int operation)
    {
        var exception = Assert.Throws<ArgumentNullException>(() => ApplyChar(operation, null!));
        Assert.Equal("str", exception.ParamName);
    }

    private static object ApplyString(int operation, string str, string open, string? close)
    {
        return operation switch
        {
            0 => str.IsWrappedWith(open, close),
            1 => str.WrapWith(open, close),
            2 => str.TrimWrapper(open, close),
            3 => str.EnsureWrappedWith(open, close),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
    }

    private static object ApplyChar(int operation, string str)
    {
        return operation switch
        {
            0 => str.IsWrappedWith('(', ')'),
            1 => str.WrapWith('(', ')'),
            2 => str.TrimWrapper('(', ')'),
            3 => str.EnsureWrappedWith('(', ')'),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
    }
}
