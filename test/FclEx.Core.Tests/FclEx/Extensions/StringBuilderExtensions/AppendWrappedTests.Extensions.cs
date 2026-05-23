namespace FclEx.Extensions.StringBuilderExtensions;

partial class AppendWrappedTests
{
    // ------------------------------------------------------------
    // Single quoted
    // ------------------------------------------------------------

    [Fact]
    public void AppendSingleQuoted_Value()
    {
        var sb = new StringBuilder();
        sb.AppendSingleQuoted("abc");
        Assert.Equal("'abc'", sb.ToString());
    }

    [Fact]
    public void AppendSingleQuoted_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendSingleQuoted((string?)null);
        Assert.Equal("''", sb.ToString());
    }

    [Fact]
    public void AppendSingleQuoted_Action()
    {
        var sb = new StringBuilder();
        var invoked = false;

        sb.AppendSingleQuoted(b =>
        {
            invoked = true;
            b.Append("abc");
        });

        Assert.True(invoked);
        Assert.Equal("'abc'", sb.ToString());
    }

    // ------------------------------------------------------------
    // Double quoted
    // ------------------------------------------------------------

    [Fact]
    public void AppendDoubleQuoted_Value()
    {
        var sb = new StringBuilder();
        sb.AppendDoubleQuoted("abc");
        Assert.Equal("\"abc\"", sb.ToString());
    }

    [Fact]
    public void AppendDoubleQuoted_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendDoubleQuoted((string?)null);
        Assert.Equal("\"\"", sb.ToString());
    }

    [Fact]
    public void AppendDoubleQuoted_Action()
    {
        var sb = new StringBuilder();
        sb.AppendDoubleQuoted(b => b.Append("abc"));
        Assert.Equal("\"abc\"", sb.ToString());
    }

    // ------------------------------------------------------------
    // Parenthesized
    // ------------------------------------------------------------

    [Fact]
    public void AppendParenthesized_Value()
    {
        var sb = new StringBuilder();
        sb.AppendParenthesized("abc");
        Assert.Equal("(abc)", sb.ToString());
    }

    [Fact]
    public void AppendParenthesized_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendParenthesized((string?)null);
        Assert.Equal("()", sb.ToString());
    }

    [Fact]
    public void AppendParenthesized_Action()
    {
        var sb = new StringBuilder();
        sb.AppendParenthesized(b => b.Append("abc"));
        Assert.Equal("(abc)", sb.ToString());
    }

    // ------------------------------------------------------------
    // Square bracketed
    // ------------------------------------------------------------

    [Fact]
    public void AppendSquareBracketed_Value()
    {
        var sb = new StringBuilder();
        sb.AppendSquareBracketed("abc");
        Assert.Equal("[abc]", sb.ToString());
    }

    [Fact]
    public void AppendSquareBracketed_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendSquareBracketed((string?)null);
        Assert.Equal("[]", sb.ToString());
    }

    [Fact]
    public void AppendSquareBracketed_Action()
    {
        var sb = new StringBuilder();
        sb.AppendSquareBracketed(b => b.Append("abc"));
        Assert.Equal("[abc]", sb.ToString());
    }

    // ------------------------------------------------------------
    // Curly braced
    // ------------------------------------------------------------

    [Fact]
    public void AppendCurlyBraced_Value()
    {
        var sb = new StringBuilder();
        sb.AppendCurlyBraced("abc");
        Assert.Equal("{abc}", sb.ToString());
    }

    [Fact]
    public void AppendCurlyBraced_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendCurlyBraced((string?)null);
        Assert.Equal("{}", sb.ToString());
    }

    [Fact]
    public void AppendCurlyBraced_Action()
    {
        var sb = new StringBuilder();
        sb.AppendCurlyBraced(b => b.Append("abc"));
        Assert.Equal("{abc}", sb.ToString());
    }

    // ------------------------------------------------------------
    // Angle bracketed
    // ------------------------------------------------------------

    [Fact]
    public void AppendAngleBracketed_Value()
    {
        var sb = new StringBuilder();
        sb.AppendAngleBracketed("abc");
        Assert.Equal("<abc>", sb.ToString());
    }

    [Fact]
    public void AppendAngleBracketed_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendAngleBracketed((string?)null);
        Assert.Equal("<>", sb.ToString());
    }

    [Fact]
    public void AppendAngleBracketed_Action()
    {
        var sb = new StringBuilder();
        sb.AppendAngleBracketed(b => b.Append("abc"));
        Assert.Equal("<abc>", sb.ToString());
    }

    // ------------------------------------------------------------
    // Backticked
    // ------------------------------------------------------------

    [Fact]
    public void AppendBackticked_Value()
    {
        var sb = new StringBuilder();
        sb.AppendBackticked("abc");
        Assert.Equal("`abc`", sb.ToString());
    }

    [Fact]
    public void AppendBackticked_NullValue()
    {
        var sb = new StringBuilder();
        sb.AppendBackticked((string?)null);
        Assert.Equal("``", sb.ToString());
    }

    [Fact]
    public void AppendBackticked_Action()
    {
        var sb = new StringBuilder();
        sb.AppendBackticked(b => b.Append("abc"));
        Assert.Equal("`abc`", sb.ToString());
    }

    // ------------------------------------------------------------
    // Fluent behavior
    // ------------------------------------------------------------

    [Fact]
    public void Methods_AreFluent()
    {
        var sb = new StringBuilder();

        var result = sb.AppendSingleQuoted("x");

        Assert.Same(sb, result);
    }

    [Fact]
    public void Methods_ComposeCorrectly()
    {
        var sb = new StringBuilder();

        sb.Append("A")
          .AppendSingleQuoted("B")
          .AppendParenthesized("C")
          .AppendSquareBracketed(b => b.Append("D"))
          .AppendCurlyBraced("E")
          .AppendAngleBracketed("F")
          .AppendBackticked("G");

        Assert.Equal("A'B'(C)[D]{E}<F>`G`", sb.ToString());
    }
}
