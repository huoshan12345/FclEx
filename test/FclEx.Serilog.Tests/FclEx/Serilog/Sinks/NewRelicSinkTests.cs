using System.Text.Json;

namespace FclEx.Serilog.Sinks;

public class NewRelicSinkTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Serialize_Test(int count)
    {
        var events = Enumerable.Range(1, count).Select(CreateLogEvent);
        var str = NewRelicSink.Serialize(events, new JsonFormatter());
        var token = JsonDocument.Parse(str);
        Assert.Equal(JsonValueKind.Array, token.RootElement.ValueKind);
    }

    [LocalOnlyFact(Skip = "No license key")]
    public async Task EmitBatchAsync_Test()
    {
        var writer = new StringWriter();
        using var x = writer.SetSelfLog();

        var sink = new NewRelicSink(licenseKey: "");
        var events = Enumerable.Range(1, 5).Select(CreateLogEvent).ToArray();
        await sink.EmitBatchAsync(events);

        var str = writer.ToString();
        Assert.Empty(str);
    }

    private static LogEvent CreateLogEvent(int number)
    {
        var template = new MessageTemplate([new TextToken("message_" + number)]);
        var props = Enumerable.Range(1, 3).Select(m => new LogEventProperty("prop" + m, new ScalarValue("value" + m)));
        return new LogEvent(Random.Shared.NextDateTime(), LogEventLevel.Information, null, template, props);
    }
}