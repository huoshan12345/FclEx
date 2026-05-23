namespace FclEx.Slack;

public class ConversationsApiTests : SlackTests
{
    [RetryFact]
    public async Task LookupChannel_Test()
    {
        var channel = await SlackApi.Conversations.LookupChannel(SlackChannels.Monitoring.Name);
        Assert.NotNull(channel);
        Assert.Equal(SlackChannels.Monitoring.Id, channel.Id);
    }

    [RetryFact]
    public async Task LookupChannels_Test()
    {
        var testCases = new[] { SlackChannels.Monitoring, SlackChannels.MonitoringLog, SlackChannels.MonitoringLogTest };
        var channels = await SlackApi.Conversations.LookupChannels(testCases.Select(m => m.Name).ToArray());
        Assert.Equal(testCases.Length, channels.Count);

        foreach (var (id, name) in testCases)
        {
            Assert.Equal(id, channels.Get(name)?.Id, () => name);
        }
    }

    [LocalOnlyTheory]
    [InlineData(SlackChannelIds.MonitoringLogTest)]
    public async Task History_Test(string channel)
    {
        var res = await SlackApi.Auth.Test();
        Assert.NotNullNorEmpty(res.UserId);

        var oldest = DateTimeOffset.UtcNow.AddDays(-3);
        var history = await SlackApi.Conversations.History(channel, oldest: oldest, retryAfterJoin: true);

        foreach (var message in history.Messages)
        {
            if (message.User == res.UserId && message.Subtype is null)
            {
                DeleteMessage(channel, message.Ts, message.Text);
            }

            if (message.ReplyCount == 0)
                continue;

            var reply = await SlackApi.Conversations.Replies(channel, message.Ts, oldest: oldest);
            foreach (var replyMessage in reply.Messages.Where(m => m.ThreadTs is not null))
            {
                if (replyMessage.User == res.UserId && replyMessage.Subtype is null)
                {
                    DeleteMessage(channel, replyMessage.Ts, replyMessage.Text);
                }
            }
        }
    }
}