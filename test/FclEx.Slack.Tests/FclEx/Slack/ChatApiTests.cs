namespace FclEx.Slack;

public class ChatApiTest : SlackTests
{
    private const int MaxWidthOfCodeBlock = 88;
    private static readonly string Channel = SlackChannels.MonitoringLogTest.Name;

    [RetryFact]
    public async Task PostMessage_Test()
    {
        var text = SlackStringBuilder.Build(m => m.AppendCodeBlock(x =>
        {
            for (var i = 0; i < MaxWidthOfCodeBlock; i++)
            {
                x.Append('x');
            }
        }));

        var message = new Message()
            .Channel(Channel)
            .AddMarkdown(text);
        var res = await SlackApi.Chat.PostMessage(message);
        DeleteMessage(res);
    }

    [RetryFact]
    public async Task PostMessage_CodeBlock_Test()
    {
        using var disposable = StringBuilder.GetCached();
        var builder = disposable.Value;

        builder.AppendLine("```");
        for (var i = 0; i < MaxWidthOfCodeBlock; i++)
        {
            builder.Append('x');
        }
        builder.AppendLine("```");

        var message = new Message()
            .Channel(Channel)
            .AddMarkdown(builder.ToString());

        var res = await SlackApi.Chat.PostMessage(message);
        DeleteMessage(res);
    }

    [RetryFact]
    public async Task PostChunked_Test()
    {
        var columns = new[] { "No.", "ClientName" };
        var rows = Enumerable.Range(1, 50).Select(m => new[] { m.ToString(), Guid.NewGuid().ToString() });
        var table = new TableData("test data title", null, columns, rows);
        var list = await SlackApi.Chat.PostChunked(Channel, table);
        list.ForEach(DeleteMessage);

        Assert.Equal(2, list.Count);
    }

    [RetryFact]
    public async Task PostChunked_Empty_Test()
    {
        var columns = new[] { "No.", "ClientName" };
        var table = new TableData("test data title", null, columns, []);
        var list = await SlackApi.Chat.PostChunked(Channel, table);
        list.ForEach(DeleteMessage);

        Assert.Equal(1, list.Count);
    }
}