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

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;
}
