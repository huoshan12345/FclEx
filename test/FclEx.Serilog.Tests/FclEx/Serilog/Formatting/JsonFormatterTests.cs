using System.Text.Json.Nodes;
using static FclEx.Serilog.ExceptionIndexOptions;

namespace FclEx.Serilog.Formatting;

[Collection(nameof(Console))]
public class JsonFormatterTests
{
    public static readonly TheoryData<bool, bool, bool, bool> IncludedFields =
    (
        from timestamp in new[] { false, true }
        from level in new[] { false, true }
        from message in new[] { false, true }
        from exception in new[] { false, true }
        select (timestamp, level, message, exception)
    ).ToTheoryData();

    [Theory]
    [MemberData(nameof(IncludedFields))]
    public void Format_OptionalFields_ProducesValidJson(bool timestamp, bool level, bool message, bool exception)
    {
        var options = new JsonFormatterOptions
        {
            UtcTimeName = timestamp ? "@t" : "",
            LogLevelName = level ? "@l" : "",
            MessageName = message ? "@m" : "",
            ExceptionName = exception ? "@x" : ""
        };
        var formatter = new JsonFormatter(options);
        var logEvent = new LogEvent(DateTimeOffset.UtcNow, LogEventLevel.Error,
            new InvalidOperationException("failure"), MessageTemplate.Empty, []);
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal(timestamp, root.TryGetProperty("@t", out _));
        Assert.Equal(level, root.TryGetProperty("@l", out _));
        Assert.Equal(message, root.TryGetProperty("@m", out _));
        Assert.Equal(exception, root.TryGetProperty("@x", out _));
        Assert.Equal(new[] { timestamp, level, message, exception }.Count(included => included), root.EnumerateObject().Count());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_OnlyProperties_ProducesValidJson(bool enableExceptionFormat)
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            UtcTimeName = "",
            LogLevelName = "",
            MessageName = "",
            ExceptionName = "",
            EnableExceptionFormat = enableExceptionFormat
        });
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Hello {Name}", "Tom");
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Equal("Tom", document.RootElement.GetProperty("name").GetString());
        Assert.Equal(2, document.RootElement.EnumerateObject().Count());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_OmittedException_SkipsExceptionFormatting(bool enableExceptionFormat)
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions
        {
            ExceptionName = "",
            EnableExceptionFormat = enableExceptionFormat
        });
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, new ThrowingMessageException(), "message");
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.False(document.RootElement.TryGetProperty("@x", out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_ExceptionFormattingFailsAfterWriting_ReplacesEntireFragment(bool includeTimestamp)
    {
        var formatter = new PartiallyFailingFormatter(new JsonFormatterOptions { UtcTimeName = includeTimestamp ? "@t" : "" });
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("failure"), "message");
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Contains("Failed to Format Exception", document.RootElement.GetProperty("@x").GetString());
        Assert.DoesNotContain("partial fragment", output.ToString());
    }

    private sealed class PartiallyFailingFormatter(JsonFormatterOptions options) : JsonFormatter(options)
    {
        protected override void WriteFormattedException(string logEventMessage, Exception ex, TextWriter output)
        {
            output.Write(",\"@x\":[\"partial fragment\"");
            throw new InvalidOperationException("formatting failed");
        }
    }

    [Fact]
    public void Format_OmittedTimestamp_ProducesValidJson()
    {
        var formatter = new JsonFormatter(new JsonFormatterOptions { UtcTimeName = "" });
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "message");
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Equal("message", document.RootElement.GetProperty("@m").GetString());
    }

    [Fact]
    public void Format_ExceptionMessageThrows_ProducesValidFallbackJson()
    {
        var formatter = new JsonFormatter();
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, new ThrowingMessageException(), "message");
        using var output = new StringWriter();

        formatter.Format(logEvent, output);

        using var document = System.Text.Json.JsonDocument.Parse(output.ToString());
        Assert.Contains("Failed to Format Exception", document.RootElement.GetProperty("@x").GetString());
    }

    private sealed class ThrowingMessageException : Exception
    {
        public override string Message => throw new InvalidOperationException("message unavailable");
    }

    public ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;

    public static readonly TheoryData<int?, bool, ExceptionIndexOptions> TestCases =
    (
        from len in new int?[] { null, 5 }
        from omitParams in new[] { true, false }
        from indexOp in new[] { None, Default }
        select (len, omitParams, indexOp)
    ).ToTheoryData();

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Format_Test(int? maxLen, bool omitParams, ExceptionIndexOptions indexOptions)
    {
        var options = new JsonFormatterOptions
        {
            ExceptionFormatOptions = new()
            {
                MaxMessageLength = maxLen,
                OmitParametersInStackTrace = omitParams,
                IndexOptions = indexOptions,
            }
        };
        try
        {
            await ExceptionCreator.Run();
        }
        catch (Exception ex)
        {
#if DEBUG
            Output?.WriteLine(ex.ToString());
            Output?.WriteLine("\n\n\n");
#endif
            await AssertLogMessage(ex);
        }

        return;

        async Task AssertLogMessage(Exception ex)
        {
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, ex,
                MessageTemplate.Empty, []);

            var formatter = new JsonFormatter(options);
#if NET6_0_OR_GREATER
            await
#endif
            using var sw = new StringWriter();

            formatter.Format(logEvent, sw);

            var str = sw.ToString();
            var token = str.ToJsonNode();
            Assert.NotNull(token);
            var xt = token[options.ExceptionName] as JsonArray;
            Assert.NotNull(xt);

            var lines = xt.ToObject<string[]>()!;
            Assert.NotEmpty(lines);

            foreach (var line in lines)
            {
                Assert.DoesNotContain("at ", line);
#if DEBUG
                Output?.WriteLine(line);
#endif
            }
        }
    }

    [Fact]
    public void FormatProperty_Test()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Information, null, "Message from {Name}", "Tom");

        var options = new JsonFormatterOptions();
        var formatter = new JsonFormatter(options);
        using var sw = new StringWriter();
        formatter.Format(logEvent, sw);

        var jsonElement = sw.ToString().ToJsonElement();
        Assert.Equal(nameof(LogEventLevel.Information), jsonElement.GetProperty(options.LogLevelName).GetString());
        Assert.Equal("Message from Tom", jsonElement.GetProperty(options.MessageName).GetString());
        Assert.Equal(logEvent.Timestamp.UtcDateTime.ToString("O"), jsonElement.GetProperty(options.UtcTimeName).GetString());
    }
}
