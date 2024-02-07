namespace FclEx.Messaging.Tests;

public class GlobalFixture : FclEx.Tests.GlobalFixture
{
    public static ConnectionSettings RmqConnection { get; } = Config.GetSection("Rmq").Get<ConnectionSettings>()!;

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public override Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}