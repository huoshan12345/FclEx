namespace FclEx.Extensions.StringBuilderExtensions;

public partial class AppendWrappedTests
{
    // ------------------------------------------------------------
    // Action<StringBuilder> overload (string)
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_String_Action_WithExplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped("(", b => b.Append("abc"), ")");

        Assert.Equal("(abc)", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_String_Action_WithImplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped("\"", b => b.Append("abc"));

        Assert.Equal("\"abc\"", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_String_Action_DelegateIsInvoked()
    {
        var sb = new StringBuilder();
        var invoked = false;

        sb.AppendWrapped("[", b =>
        {
            invoked = true;
            b.Append('x');
        }, "]");

        Assert.True(invoked);
        Assert.Equal("[x]", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_String_Action_NullAction()
    {
        var sb = new StringBuilder();
        var ex = Assert.Throws<ArgumentNullException>(() => sb.AppendWrapped("[", (Action<StringBuilder>?)null!, "]"));
        Assert.Contains("'appendContent'", ex.Message);
    }

    [Fact]
    public void AppendWrapped_String_Action_EmptyOpenOrClose()
    {
        // the open is empty
        {
            var sb = new StringBuilder();
            var ex = Assert.Throws<ArgumentException>(() => sb.AppendWrapped("", m => { }, ")"));
            Assert.Contains("'open'", ex.Message);
        }

        // the close is empty
        {
            var sb = new StringBuilder();
            var ex = Assert.Throws<ArgumentException>(() => sb.AppendWrapped("(", m => { }, ""));
            Assert.Contains("'close'", ex.Message);
        }

        // the open is empty and close is null (which defaults to open)
        {
            var sb = new StringBuilder();
            var ex = Assert.Throws<ArgumentException>(() => sb.AppendWrapped("", m => { }));
            Assert.Contains("'open'", ex.Message);
        }

        // the open is empty and close is empty
        {
            var sb = new StringBuilder();
            var ex = Assert.Throws<ArgumentException>(() => sb.AppendWrapped("", m => { }, ""));
            Assert.Contains("'open'", ex.Message);
        }
    }

    // ------------------------------------------------------------
    // Action<StringBuilder> overload (char)
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_Char_Action_WithExplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped('(', b => b.Append("abc"), ')');

        Assert.Equal("(abc)", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_Char_Action_WithImplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped('"', b => b.Append("abc"));

        Assert.Equal("\"abc\"", sb.ToString());
    }
    
    [Fact]
    public void AppendWrapped_Char_Action_NullAction()
    {
        var sb = new StringBuilder();
        var ex = Assert.Throws<ArgumentNullException>(() => sb.AppendWrapped('"', (Action<StringBuilder>?)null!, '"'));
        Assert.Contains("'appendContent'", ex.Message);
    }

    // ------------------------------------------------------------
    // string value overload (string)
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_String_Value_WithExplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped("(", "abc", ")");

        Assert.Equal("(abc)", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_String_Value_WithImplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped("\"", "abc");

        Assert.Equal("\"abc\"", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_String_Value_NullValue()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped("(", (string?)null, ")");

        Assert.Equal("()", sb.ToString());
    }

    // ------------------------------------------------------------
    // string value overload (char)
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_Char_Value_WithExplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped('(', "abc", ')');

        Assert.Equal("(abc)", sb.ToString());
    }

    [Fact]
    public void AppendWrapped_Char_Value_WithImplicitClose()
    {
        var sb = new StringBuilder();

        sb.AppendWrapped('"', "abc");

        Assert.Equal("\"abc\"", sb.ToString());
    }

    // ------------------------------------------------------------
    // Fluent chaining
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_IsFluent()
    {
        var sb = new StringBuilder();

        var result = sb.AppendWrapped("(", "abc", ")");

        Assert.Same(sb, result);
    }

    // ------------------------------------------------------------
    // Multi-call composition
    // ------------------------------------------------------------

    [Fact]
    public void AppendWrapped_MultipleCalls_ComposeCorrectly()
    {
        var sb = new StringBuilder();

        sb.Append("X")
            .AppendWrapped("(", "a", ")")
            .Append("Y")
            .AppendWrapped("[", b => b.Append("b"), "]");

        Assert.Equal("X(a)Y[b]", sb.ToString());
    }
}