namespace FclEx.Cooperation.Slack;

public class ConversationsApiTests : IAssemblyFixture<GlobalFixture>
{
    [RetryFact]
    public async Task LookupChannel_Test()
    {
        if (Environment.Version.Major != 8)
            return;

        var channel = await SlackApi.Conversations.LookupChannel("test-monitoring");
        Assert.NotNull(channel);
        Assert.Equal(SlackChannels.TestMonitoring, channel.Id);
    }

    [RetryFact]
    public async Task LookupChannels_Test()
    {
        if (Environment.Version.Major != 8)
            return;

        var channels = await SlackApi.Conversations.LookupChannels(new[] { "test-monitoring", "help-monitoring" });
        Assert.Equal(2, channels.Count);

        Assert.Equal(SlackChannels.TestMonitoring, channels.GetValueOrDefault("test-monitoring")?.Id);
        Assert.Equal(SlackChannels.HelpMonitoring, channels.GetValueOrDefault("help-monitoring")?.Id);
    }
}