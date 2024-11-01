// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace FclEx.Tests;

public class GlobalFixture : IAsyncLifetime
{
    public static IConfigurationRoot BuildConfig()
    {
        var machineName = Environment.MachineName.ToLower();
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .AddJsonFile($"appsettings.{machineName}.json", true, false)
            .AddEnvironmentVariables("FclEx_")
            .Build();
    }

    public static IConfigurationRoot Config { get; } = BuildConfig();
    public static AppSettings AppSettings { get; } = Config.Get<AppSettings>()!;

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}

public class AppSettings
{
    public SlackConfig Slack { get; set; } = default!;
    public JiraConfig Jira { get; set; } = default!;
    public NewRelicConfig NewRelic { get; set; } = default!;
}

public class SlackConfig
{
    /// <summary>
    /// Bot tokens represent a bot associated with the app installed in a workspace. <br/>
    /// Unlike user tokens, they're not tied to a user's identity; they're just tied to your app.
    /// </summary>
    public string BotToken { get; set; } = "";
    /// <summary>
    /// Slack signs the requests we send you using this secret. <br/>
    /// Confirm that each request comes from Slack by verifying its unique signature.
    /// </summary>
    public string SigningSecret { get; set; } = "";
}

public class JiraConfig
{
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    /// <summary>
    /// The base URL is the URL via which users access Jira applications.
    /// </summary>
    public string BaseUrl { get; set; } = "";
}

public class NewRelicConfig
{
    public string ApiKey { get; set; } = "";
    public string LicenseKey { get; set; } = "";
    public string AccountId { get; set; } = "";
}
