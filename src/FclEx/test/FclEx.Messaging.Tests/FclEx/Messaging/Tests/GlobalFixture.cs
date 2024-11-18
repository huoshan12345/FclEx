namespace FclEx.Messaging.Tests;

public class GlobalFixture : FclEx.Tests.GlobalFixture
{
    public static ConnectionSettings RmqConnection { get; } = Config.GetSection("Rmq").Get<ConnectionSettings>()!;
}