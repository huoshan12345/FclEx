using Microsoft.Extensions.Configuration;
using Xunit;

public static class GlobalConstants
{
    public static IConfigurationRoot BuildConfig()
    {
        var machineName = Environment.MachineName.ToLower();
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", true, false)
            .AddJsonFileIf(Startup.IsGithubAction, "appsettings.github.json", true, false)
            .AddJsonFile($"appsettings.{machineName}.json", true, false)
            .Build();
    }

    public static IConfigurationRoot Config { get; } = BuildConfig();
}