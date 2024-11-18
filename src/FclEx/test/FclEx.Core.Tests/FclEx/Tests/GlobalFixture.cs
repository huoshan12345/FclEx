using FclEx.Xunit;

namespace FclEx.Tests;

public class GlobalFixture : IAsyncLifetime
{
    public static IConfigurationRoot BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .AddEnvironmentVariables("FclEx_");

        if (TestHelper.IsGithubAction)
        {
            builder.AddJsonFile("appsettings.github.json", true, false);
        }
        else
        {
            var machineName = Environment.MachineName.ToLower();
            builder.AddJsonFile($"appsettings.{machineName}.json", true, false);
        }

        return builder.Build();
    }

    public static IConfigurationRoot Config { get; } = BuildConfig();

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
