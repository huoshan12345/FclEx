using Microsoft.Extensions.Configuration;
using SlackNet;
using SlackNet.WebApi;

namespace FclEx.Slack;

public record SlackMessage(string Channel, string Ts, string Text);

public class SlackConfig
{
    /// <summary>
    /// Bot tokens represent a bot associated with the app installed in a workspace. <br/>
    /// Unlike user tokens, they're not tied to a user's identity; they're just tied to your app.
    /// </summary>
    public string BotToken { get; set; } = "";
}

public readonly record struct SlackObject(string Id, string Name);

public static class SlackChannelIds
{
    public const string Monitoring = "C0AQ8NY5JAF";
    public const string MonitoringLog = "C0AQBH437MK";
    public const string MonitoringLogTest = "C0AQQ6NFXT6";
}

public static class SlackChannels
{
    public static readonly SlackObject Monitoring = new(SlackChannelIds.Monitoring, "monitoring");
    public static readonly SlackObject MonitoringLog = new(SlackChannelIds.MonitoringLog, "monitoring-log");
    public static readonly SlackObject MonitoringLogTest = new(SlackChannelIds.MonitoringLogTest, "monitoring-log-test");
}

public class SlackFixture : CoreTestsFixture
{
    private static readonly ConcurrentBag<SlackMessage> Messages = [];

    public static SlackConfig SlackConfig { get; } = Config.GetSection("Slack").Get<SlackConfig>()!;

    public static readonly IServiceProvider Services = new ServiceCollection()
        .AddSingleton(SlackConfig)
        .AddSlackNetExt(c => c.UseApiToken(SlackConfig.BotToken))
        .BuildServiceProvider();

    public static ISlackApiClient SlackApi => Services.GetRequiredService<ISlackApiClient>();

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await DeleteMessagesAsync();
    }

    public static void DeleteMessage(SlackMessage message) => Messages.Add(message);
    public static void DeleteMessage(string channel, string ts, string text) => DeleteMessage(new SlackMessage(channel, ts, text));
    public static void DeleteMessage(PostMessageResponse response)
        => DeleteMessage(new SlackMessage(response.Channel, response.Ts, response.Message.Text));

    private static async Task DeleteMessagesAsync()
    {
        foreach (var message in Messages)
        {
            try
            {
                await SlackApi.Chat.Delete(message.Ts, message.Channel);
            }
            catch (SlackException ex)
            {
                Console.WriteLine($"Failed to delete message due to {ex.ErrorCode}, text: {message.Text}");
            }
        }
    }
}
