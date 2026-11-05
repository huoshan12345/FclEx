using System.Text.Json.Nodes;
using static FclEx.Serilog.ExceptionIndexOptions;

namespace FclEx.Serilog.Formatting;

[Collection(nameof(Console))]
public class JsonFormatterTests
{
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
