using static FclEx.Serilog.Formatting.ExceptionPrintOptions;
using static FclEx.Serilog.Formatting.ExceptionWriteIndexOptions;

namespace FclEx.Serilog.Formatting;

public class JsonFormatterTests
{
    private readonly ITestOutputHelper _output;

    public JsonFormatterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static readonly IEnumerable<object[]> TestCases =
        from len in new int?[] { null, 5 }
        from skipParas in new[] { true, false }
        from op in new[] { SingleMessage, MessagesForEachLine }
        from indexOp in new[] { DonotWrite, Default }
        select new object[] { len, skipParas, op, indexOp };

    [Theory]
    [MemberData(nameof(TestCases))]
    public async Task Format_Test(int? maxLen, bool skipParas, ExceptionPrintOptions printOptions, ExceptionWriteIndexOptions indexOptions)
    {
        var options = new JsonFormatterOptions
        {
            ExceptionPrintOptions = printOptions,
            ExceptionFormatOptions = new()
            {
                MaxMessageLength = maxLen,
                SkipParasInStackTrace = skipParas,
                WriteIndexOptions = indexOptions
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
            _output.WriteLine(ex.ToString());
            _output.WriteLine("\n\n\n");

            await AssertLogMessage(ex);
            AssertConsoleMessage(ex);
        }

        async Task AssertLogMessage(Exception ex)
        {
            var logEvent = new LogEvent(DateTimeOffset.Now, LogEventLevel.Error, ex,
                MessageTemplate.Empty, Enumerable.Empty<LogEventProperty>());

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
                _output.WriteLine(line);
            }
        }

        void AssertConsoleMessage(Exception ex)
        {
            var original = ex.ToString();

            var str = writer.ToString();
            Assert.NotEmpty(str);

            var lines = str.SplitToLines();
            if (printOptions == SingleMessage)
            {
                Assert.Single(lines);

                var actual = JToken.Parse(lines[0])[exName]?.Value<string>();
                Assert.Equal(original, actual);
            }
            else if (printOptions == MessagesForEachLine)
            {
                var originalLines = original.SplitToLines();

                Assert.Equal(originalLines.Length, lines.Length);

                foreach (var (line, originalLine) in lines.Zip(originalLines))
                {
                    var actual = JToken.Parse(line)[exName]?.Value<string>();
                    Assert.Equal(originalLine, actual);
                }
            }
        }
    }
}