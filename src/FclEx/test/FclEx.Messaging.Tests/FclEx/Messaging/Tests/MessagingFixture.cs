namespace FclEx.Messaging.Tests;

public class MessagingFixture : GlobalFixture
{
    public static ConnectionSettings RmqConnection { get; } = Config.GetSection("Rmq").Get<ConnectionSettings>()!;
}