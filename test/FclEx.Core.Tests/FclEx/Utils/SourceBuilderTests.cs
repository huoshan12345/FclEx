namespace FclEx.Utils;

public class SourceBuilderTests
{
    [Fact]
    public void WriteLine_AppendsLine()
    {
        using var sb = new SourceBuilder();
        sb.WriteLine("Hello");

        var result = sb.ToString();

        Assert.Contains("Hello", result);
        Assert.EndsWith(sb.NewLine, result);
    }

    [Fact]
    public void Write_AppendsWithoutNewline()
    {
        using var sb = new SourceBuilder();
        sb.Write("Hello");
        sb.Write("World");

        var result = sb.ToString();

        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void WriteLineNoTabs_DoesNotIndent()
    {
        using var sb = new SourceBuilder();
        sb.Indent();
        sb.WriteLineNoTabs("Hello");

        var result = sb.ToString().TrimEnd();

        Assert.Equal("Hello", result); // no spaces
    }

    [Fact]
    public void IndentAndUnindent_WorksCorrectly()
    {
        using var sb = new SourceBuilder();
        sb.Indent()
            .WriteLine("Indented")
            .Unindent()
            .WriteLine("NotIndented");

        var text = sb.ToString();
        var result = text.Split([sb.NewLine], StringSplitOptions.None);

        Assert.StartsWith("    ", result[0]); // 4 spaces indentation
        Assert.False(result[1].StartsWith("    "));
    }

    [Fact]
    public void RemoveExtraNewLines_RemovesExcess()
    {
        using var sb = new SourceBuilder();
        sb.WriteLine("Line1")
          .WriteLine()
          .WriteLine()
          .RemoveExtraNewLines();

        var result = sb.ToString();

        Assert.EndsWith(sb.NewLine, result);
        Assert.DoesNotContain(sb.NewLine + sb.NewLine + sb.NewLine, result);
    }

    [Fact]
    public void EndsWith_ReturnsTrue_WhenSuffixMatches()
    {
        using var sb = new SourceBuilder();
        sb.Write("HelloWorld");

        Assert.True(sb.EndsWith("World"));
    }

    [Fact]
    public void EndsWith_ReturnsFalse_WhenSuffixDoesNotMatch()
    {
        using var sb = new SourceBuilder();
        sb.Write("HelloWorld");

        Assert.False(sb.EndsWith("Hello"));
    }

    [Fact]
    public void ToString_ReturnsBuiltString()
    {
        using var sb = new SourceBuilder();
        sb.WriteLine("Hello");

        var result = sb.ToString();

        Assert.Equal("Hello" + sb.NewLine, result);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var sb = new SourceBuilder();
        sb.Dispose();
        sb.Dispose(); // should not throw
    }
}
