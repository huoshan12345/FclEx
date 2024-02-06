using Atlassian.Jira;
using SlackNet;
using SlackNet.WebApi;

namespace FclEx.Cooperation;

public record SlackMessage(string Channel, string Ts, string Text);

public class GlobalFixture : Tests.GlobalFixture
{
    private static readonly ConcurrentBag<SlackMessage> Messages = [];

    public static readonly IServiceProvider Provider = new ServiceCollection()
        .AddSingleton(AppSettings)
        .AddNewRelicClient(AppSettings.NewRelic.ApiKey)
        .AddSlackNetExt(c => c.UseApiToken(AppSettings.Slack.BotToken))
        .AddSingleton(Jira.CreateRestClient(AppSettings.Jira.BaseUrl, AppSettings.Jira.UserName, AppSettings.Jira.Password))
        .BuildServiceProvider();

    public static ISlackApiClient SlackApi => Provider.GetRequiredService<ISlackApiClient>();
    public static Jira JiraApi => Provider.GetRequiredService<Jira>();
    public static NewRelicClient NewRelicApi => Provider.GetRequiredService<NewRelicClient>();

    public override Task InitializeAsync()
    {
        Messages.Clear();
        return Task.CompletedTask;
    }

    public override async Task DisposeAsync()
    {
        await DeleteMessages();
    }

    public static void DeleteMessage(SlackMessage message) => Messages.Add(message);
    public static void DeleteMessage(string channel, string ts, string text) => DeleteMessage(new SlackMessage(channel, ts, text));
    public static void DeleteMessage(PostMessageResponse response)
        => DeleteMessage(new SlackMessage(response.Channel, response.Ts, response.Message.Text));

    private static async Task DeleteMessages()
    {
        foreach (var message in Messages)
        {
            try
            {
                await SlackApi.Chat.Delete(message.Ts, message.Channel, true);
            }
            catch (SlackException ex)
            {
                Console.WriteLine($"Failed to delete message due to {ex.ErrorCode}, text: {message.Text}");
            }
        }
    }
}