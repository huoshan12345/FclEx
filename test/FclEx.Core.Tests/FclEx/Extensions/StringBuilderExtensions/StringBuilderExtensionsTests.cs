namespace FclEx.Extensions.StringBuilderExtensions;

public class StringBuilderExtensionsTests
{
    [Fact]
    public void Build_ReturnsCallbackContentWithAnInitiallyEmptyBuilder()
    {
        Assert.Equal("first", StringBuilder.Build(builder => builder.Append("first")));
        Assert.Equal("", StringBuilder.Build(builder => Assert.Equal(0, builder.Length)));
    }

    [Fact]
    public void Build_NestedCallsUseIndependentBuilders()
    {
        var result = StringBuilder.Build(outer =>
        {
            outer.Append("outer:");
            outer.Append(StringBuilder.Build(inner => inner.Append("inner")));
        });
        Assert.Equal("outer:inner", result);
    }

    [Fact]
    public void Build_PropagatesCallbackFailureAndClearsBuilderForNextCall()
    {
        var failure = new InvalidOperationException("callback failed");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => StringBuilder.Build(builder =>
        {
            builder.Append("partial content");
            throw failure;
        })));
        Assert.Equal("next", StringBuilder.Build(builder => builder.Append("next")));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenSubstringMatches()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.True(sb.Equals("Hello".AsSpan(), 0));
        Assert.True(sb.Equals("World".AsSpan(), 5));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSubstringDoesNotMatch()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.False(sb.Equals("Test".AsSpan(), 0));
        Assert.False(sb.Equals("Hello".AsSpan(), 1));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSpanTooLong()
    {
        var sb = new StringBuilder("Hi");

        Assert.False(sb.Equals("LongerText".AsSpan(), 0));
    }

    [Fact]
    public void Equals_ShouldThrow_WhenBuilderIsNull()
    {
        StringBuilder? sb = null;

        Assert.Throws<ArgumentNullException>(() => sb!.Equals("abc".AsSpan(), 0));
    }

    [Fact]
    public void Equals_ShouldThrow_WhenStartIndexIsNegative()
    {
        var sb = new StringBuilder("Hello");

        Assert.Throws<ArgumentOutOfRangeException>(() => sb.Equals("He".AsSpan(), -1));
    }

    [Fact]
    public void StartsWith_ShouldReturnTrue_WhenPrefixMatches()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.True(sb.StartsWith("Hello".AsSpan()));
    }

    [Fact]
    public void StartsWith_ShouldReturnFalse_WhenPrefixDoesNotMatch()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.False(sb.StartsWith("World".AsSpan()));
    }

    [Fact]
    public void StartsWith_ShouldReturnFalse_WhenSpanLongerThanBuilder()
    {
        var sb = new StringBuilder("Hi");

        Assert.False(sb.StartsWith("Hello".AsSpan()));
    }

    [Fact]
    public void EndsWith_ShouldReturnTrue_WhenSuffixMatches()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.True(sb.EndsWith("World".AsSpan()));
    }

    [Fact]
    public void EndsWith_ShouldReturnFalse_WhenSuffixDoesNotMatch()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.False(sb.EndsWith("Hello".AsSpan()));
    }

    [Fact]
    public void EndsWith_ShouldReturnFalse_WhenSpanLongerThanBuilder()
    {
        var sb = new StringBuilder("Hi");

        Assert.False(sb.EndsWith("Hello".AsSpan()));
    }

    [Fact]
    public void StartsWith_And_EndsWith_ShouldWorkWithEmptySpan()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.True(sb.StartsWith(ReadOnlySpan<char>.Empty));
        Assert.True(sb.EndsWith(ReadOnlySpan<char>.Empty));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenEmptySpan()
    {
        var sb = new StringBuilder("HelloWorld");

        Assert.True(sb.Equals(ReadOnlySpan<char>.Empty, 0));
        Assert.True(sb.Equals(ReadOnlySpan<char>.Empty, 5));
    }
}
