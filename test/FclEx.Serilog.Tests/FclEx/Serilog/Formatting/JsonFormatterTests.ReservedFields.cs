using System.Text.Json;

namespace FclEx.Serilog.Formatting;

public partial class JsonFormatterTests
{
    public static TheoryData<int, int> DuplicateBasicFieldCases =>
        (from first in Enumerable.Range(0, 4)
         from second in Enumerable.Range(first + 1, 3 - first)
         select (first, second)).ToTheoryData();

    [Theory]
    [MemberData(nameof(DuplicateBasicFieldCases))]
    public void Format_DuplicateBasicFieldNames_RejectsBeforeWriting(int first, int second)
    {
        var options = new JsonFormatterOptions();
        var formatter = new JsonFormatter(options);
        Action<string>[] setters =
        [
            value => options.UtcTimeName = value, value => options.LogLevelName = value,
            value => options.MessageName = value, value => options.ExceptionName = value
        ];
        // Changes made after construction must also be checked.
        setters[first]("duplicate");
        setters[second]("duplicate");
        using var output = new StringWriter();

        Assert.Throws<InvalidOperationException>(() => formatter.Format(
            SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "message"), output));

        Assert.Equal("", output.ToString());
    }

    [Theory]
    [InlineData("time")]
    [InlineData("level")]
    [InlineData("message")]
    [InlineData("exception")]
    public void Format_PropertiesCannotOverwriteBasicFields(string field)
    {
        var options = new JsonFormatterOptions
        {
            UtcTimeName = "time", LogLevelName = "level", MessageName = "message", ExceptionName = "exception"
        };
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, new InvalidOperationException("failure"), "original");
        logEvent.AddOrUpdateProperty(new LogEventProperty(char.ToUpperInvariant(field[0]) + field[1..], new ScalarValue("overwritten")));
        using var output = new StringWriter();

        new JsonFormatter(options).Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        var basic = Assert.Single(document.RootElement.EnumerateObject(), property => property.Name == field);
        Assert.NotEqual("\"overwritten\"", basic.Value.GetRawText());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Format_LinkingMetadataCannotOverwriteBasicFields(bool hasException)
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error,
            hasException ? new InvalidOperationException("failure") : null, "original");
        logEvent.AddOrUpdateProperty(new LogEventProperty("newrelic.linkingmetadata", new DictionaryValue(
            new[] { "Time", "Level", "Message", "Exception", "TraceId" }.Select(name =>
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(new ScalarValue(name), new ScalarValue("metadata"))))));
        var options = new JsonFormatterOptions
        {
            UtcTimeName = "time", LogLevelName = "level", MessageName = "message", ExceptionName = "exception"
        };
        using var output = new StringWriter();

        new JsonFormatter(options).Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("Error", document.RootElement.GetProperty("level").GetString());
        Assert.Equal("original", document.RootElement.GetProperty("message").GetString());
        Assert.Equal(hasException, document.RootElement.TryGetProperty("exception", out _));
        Assert.Equal("metadata", document.RootElement.GetProperty("traceId").GetString());
        Assert.Equal(document.RootElement.EnumerateObject().Count(), document.RootElement.EnumerateObject().Select(property => property.Name).Distinct().Count());
    }

    [Fact]
    public void Format_ReservedNameCheckRunsAfterAtEscaping()
    {
        var logEvent = SerilogHelper.CreateLogEvent(LogEventLevel.Error, null, "original");
        logEvent.AddOrUpdateProperty(new LogEventProperty("@l", new ScalarValue("user value")));
        using var output = new StringWriter();

        new JsonFormatter().Format(logEvent, output);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("Error", document.RootElement.GetProperty("@l").GetString());
        Assert.Equal("user value", document.RootElement.GetProperty("@@l").GetString());
    }
}
