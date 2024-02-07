namespace FclEx.Cooperation.Slack;

public class ConversationsApiTests : IAssemblyFixture<GlobalFixture>
{
    [RetryFact]
    public async Task LookupChannel_Test()
    {
        var channel = await SlackApi.Conversations.LookupChannel(SlackChannelNames.MonitoringTest);
        Assert.NotNull(channel);
        Assert.Equal(SlackChannelIds.MonitoringTest, channel.Id);
    }

    [RetryFact]
    public async Task LookupChannels_Test()
    {
        var channels = await SlackApi.Conversations.LookupChannels(new[] { SlackChannelNames.MonitoringTest, SlackChannelNames.Monitoring });
        Assert.Equal(2, channels.Count);

        Assert.Equal(SlackChannelIds.MonitoringTest, channels.Get(SlackChannelNames.MonitoringTest)?.Id);
        Assert.Equal(SlackChannelIds.Monitoring, channels.Get(SlackChannelNames.Monitoring)?.Id);
    }
}