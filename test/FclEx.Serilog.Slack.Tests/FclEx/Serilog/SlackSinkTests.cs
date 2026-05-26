using Serilog.Events;

namespace FclEx.Serilog;

public class SlackSinkTests : SlackTests
{
    [LocalOnlyFact]
    public async Task EmitBatchAsync_Test()
    {
        var start = DateTimeOffset.Now;
        var channel = SlackChannels.MonitoringLogTest;

        var writer = new StringWriter();
        using var x = writer.SetSelfLog();

        var sink = new SlackSink(SlackTestsFixture.SlackConfig.BotToken, channel.Name);
        var ex = new Exception("test error").SetStackTrace("random stack trace");
        var events = Enumerable.Range(1, 2)
            .Select(m => LogEventExtensionsTests.CreateLogEvent(LogEventLevel.Information, ex, "Message {Index}", m))
            .ToArray();

        await sink.EmitBatchAsync(events);

        var str = writer.ToString();
        Assert.Empty(str);

        var history = await SlackApi.Conversations.History(channel.Id, oldest: start, inclusive: true, retryAfterJoin: true);

        history.Messages.ForEach(m => DeleteMessage(channel.Id, m.Ts, m.Text));

        foreach (var logEvent in events)
        {
            Assert.Contains(history.Messages, m => m.Text.Contains(logEvent.RenderMessage("l")));
        }
    }
}
