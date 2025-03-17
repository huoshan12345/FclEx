using static FclEx.Serilog.ExceptionIndexOptions;

namespace FclEx.Serilog.Formatting;

[Collection(nameof(Console))]
public class JsonFormatterTests
{
    private readonly ITestOutputHelper _output;

    public JsonFormatterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static readonly IEnumerable<object[]> TestCases =
        from len in new int?[] { null, 5 }
        from omitParams in new[] { true, false }
        from indexOp in new[] { None, Default }
        select new object[] { len, omitParams, indexOp };

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

        var exName = options.ExceptionName;

        await using var writer = new StringWriter();
        using var x = writer.SetConsole();

        try
        {
            await ExceptionCreator.Run();
        }
        catch (Exception ex)
        {
#if DEBUG
            _output.WriteLine(ex.ToString());
            _output.WriteLine("\n\n\n");
#endif
            await AssertLogMessage(ex);

            var str = writer.ToString();
            Assert.Empty(str);
        }

        return;

        async Task AssertLogMessage(Exception ex)
        {
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, ex,
                MessageTemplate.Empty, []);

            var formatter = new JsonFormatter(options);
            await using var sw = new StringWriter();

            formatter.Format(logEvent, sw);

            var str = sw.ToString();
            var token = JToken.Parse(str);
            var xt = token[options.ExceptionName] as JArray;
            Assert.NotNull(xt);

            var lines = xt.ToObject<string[]>()!;
            Assert.NotEmpty(lines);

            foreach (var line in lines)
            {
                Assert.DoesNotContain("at ", line);
#if DEBUG
                _output.WriteLine(line);
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
        Assert.Equal(LogEventLevel.Information.ToString(), jsonElement.GetProperty(options.LogLevelName).GetString());
        Assert.Equal("Message from Tom", jsonElement.GetProperty(options.MessageName).GetString());
        Assert.Equal(logEvent.Timestamp.UtcDateTime.ToString("O"), jsonElement.GetProperty(options.UtcTimeName).GetString());
    }
}