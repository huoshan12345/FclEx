using FclEx.Abp.RabbitMQ;
using FclEx.Xunit;
using Microsoft.Extensions.Configuration;

public static class GlobalConstants
{
    public static IConfigurationRoot BuildConfig()
    {
        var env = Environment.MachineName.ToLower();
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, false)
            .AddJsonFileIf(TestHelper.IsGithubAction, "appsettings.github.json", true, false)
            .AddJsonFile($"appsettings.{env}.json", true, false)
            .Build();
    }

    public static IConfiguration Config { get; } = BuildConfig();

    public static ConnectionSettings RmqConnection { get; } = Config.GetRequiredValue<ConnectionSettings>("Rmq");
}