using System.Text.Json;

namespace FclEx.Serilog.Formatting;

public partial class JsonFormatterTests
{
    public static TheoryData<string> JsonStringCases => Enumerable.Range(0, 32)
        .Select(value => "before" + (char)value + "after")
        .Concat(new[] { "", "quote\"slash\\", "汉字🙂" }).ToTheoryData();

    [Theory]
    [MemberData(nameof(JsonStringCases))]
    public void WriteJsonString_EscapedContent_RoundTripsInsideQuotes(string value)
    {
        using var output = new StringWriter();
        output.Write('"');

        JsonFormatter.WriteJsonString(value, output);
        output.Write('"');

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(value, document.RootElement.GetString());
    }

    [Theory]
    [InlineData(null, "abcdef")]
    [InlineData(-1, "")]
    [InlineData(0, "")]
    [InlineData(3, "abc...")]
    [InlineData(6, "abcdef")]
    public void Format_MessageContentLimit_ExcludesTrailingDots(int? limit, string expected)
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            ExceptionFormatOptions = new ExceptionFormatOptions { MaxMessageLength = limit }
        });
        using var output = new StringWriter();

        formatter.Format(SerilogHelper.CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("abcdef"), "unrelated"), output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("InvalidOperationException: " + expected, document.RootElement.GetProperty("@x")[0].GetString());
    }

    [Theory]
    [InlineData(true, "InvalidOperationException")]
    [InlineData(false, "InvalidOperationException: abc...")]
    public void Format_DuplicateMessageCheckRunsBeforeTruncation(bool omitDuplicate, string expected)
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            ExceptionFormatOptions = new ExceptionFormatOptions { OmitMessageIfExists = omitDuplicate, MaxMessageLength = 3 }
        });
        using var output = new StringWriter();

        formatter.Format(SerilogHelper.CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("abcdef"), "contains abcdef"), output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(expected, document.RootElement.GetProperty("@x")[0].GetString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_StackFrames_RespectsParameterOmission(bool omitParameters)
    {
        var exception = new InvalidOperationException("failure")
            .SetStackTrace("   at App.Save(System.String value)\n   at App.Main()");
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            ExceptionFormatOptions = new ExceptionFormatOptions { OmitParametersInStackTrace = omitParameters }
        });
        using var output = new StringWriter();

        formatter.Format(SerilogHelper.CreateLogEvent(LogEventLevel.Error, exception, "message"), output);

        using var document = JsonDocument.Parse(output.ToString());
        var lines = document.RootElement.GetProperty("@x").EnumerateArray().Select(value => value.GetString()).ToArray();
        Assert.Contains(omitParameters ? "App.Save" : "App.Save(System.String value)", lines);
    }
}
