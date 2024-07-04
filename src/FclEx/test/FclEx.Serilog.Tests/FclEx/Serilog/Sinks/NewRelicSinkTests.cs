namespace FclEx.Serilog.Sinks;

public class NewRelicSinkTests : IAssemblyFixture<GlobalFixture>
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Serialize_Test(int count)
    {
        var events = Enumerable.Range(1, count).Select(m => CreateLogEvent(m));
        var str = NewRelicSink.Serialize(events, new JsonFormatter());
        var token = JToken.Parse(str);
        Assert.Equal(JTokenType.Array, token.Type);
    }

    [Fact]
    public async Task EmitBatchAsync_Test()
    {
        var writer = new StringWriter();
        using var x = writer.SetSelfLog();

        var sink = new NewRelicSink(GlobalFixture.AppSettings.NewRelic.LicenseKey);
        var events = Enumerable.Range(1, 5).Select(m => CreateLogEvent(m)).ToArray();
        await sink.EmitBatchAsync(events);

        var str = writer.ToString();
        Assert.Empty(str);
    }

    private static LogEvent CreateLogEvent(int number)
    {
        var template = new MessageTemplate(new TextToken("message_" + number).Yield());
        var props = Enumerable.Range(1, 3).Select(m => new LogEventProperty("prop" + m, new ScalarValue("value" + m)));
        return new LogEvent(Random.Shared.NextDateTime(), LogEventLevel.Information, null, template, props);
    }
}