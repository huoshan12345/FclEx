using SlackNet;
using SlackNet.WebApi;

namespace FclEx.Cooperation.Slack;

public class ChatApiTest : IAssemblyFixture<GlobalFixture>
{
    private const int MaxWidthOfCodeBlock = 88;

    internal const string Channel = "monitoring-test";

    [LocalOnlyFact]
    public async Task PostMessage_Test()
    {
        var text = SlackStringBuilder.Build(m => m.RenderCodeBlock(x =>
        {
            for (var i = 0; i < MaxWidthOfCodeBlock; i++)
            {
                x.Append('x');
            }
        }));

        var message = new Message()
            .Channel(Channel)
            .AddMarkdown(text);
        await SlackApi.Chat.PostMessage(message);
    }

    [LocalOnlyFact]
    public async Task PostMessage_CodeBlock_Test()
    {
        using var disposable = ObjectPoolHelper.StringBuilderPool.GetAsDisposable();
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

        await SlackApi.Chat.PostMessage(message);
    }

    [Fact]
    public async Task PostChunked_Test()
    {
        var columns = new[] { "No.", "ClientName" };
        var rows = Enumerable.Range(1, 50).Select(m => new[] { m.ToString(), Guid.NewGuid().ToString() });
        var table = new TableData("test data title", null, columns, rows);
        var list = await SlackApi.Chat.PostChunked(Channel, table);
        list.ForEach(DeleteMessage);

        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task PostChunked_Empty_Test()
    {
        var columns = new[] { "No.", "ClientName" };
        var table = new TableData("test data title", null, columns, Enumerable.Empty<string?[]>());
        var list = await SlackApi.Chat.PostChunked(Channel, table);
        list.ForEach(DeleteMessage);

        Assert.Equal(1, list.Count);
    }

    [LocalOnlyTheory]
    [InlineData(SlackChannelIds.MonitoringTest)]
    public async Task History_Test(string channel)
    {
        var res = await SlackApi.Auth.Test();
        AssertExt.NotEmpty(res.UserId);

        var ts = DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();
        var tsStr = ts.ToString("f6");
        var history = await History();

        foreach (var message in history.Messages)
        {
            if (message.User == res.UserId && message.Subtype is null)
            {
                DeleteMessage(new SlackMessage(channel, message.Ts, message.Text));
            }

            if (message.ReplyCount == 0)
                continue;

            var reply = await SlackApi.Conversations.Replies(channel, message.Ts, oldestTs: tsStr, limit: 100);
            foreach (var replyMessage in reply.Messages.Where(m => m.ThreadTs is not null))
            {
                if (replyMessage.User == res.UserId && replyMessage.Subtype is null)
                {
                    DeleteMessage(new SlackMessage(channel, replyMessage.Ts, replyMessage.Text));
                }
            }
        }

        async Task<ConversationHistoryResponse> History()
        {
            try
            {
                return await SlackApi.Conversations.History(channel, oldestTs: tsStr, limit: 1000);
            }
            catch (SlackException ex) when (ex.ErrorCode == "not_in_channel")
            {
                await SlackApi.Conversations.Join(channel);
                return await History();
            }
        }
    }
}