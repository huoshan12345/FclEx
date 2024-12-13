namespace FclEx.RabbitMQ;

public class RabbitMQFixture : GlobalFixture
{
    public static ConnectionSettings RmqConnection { get; } = Config.GetSection("Rmq").Get<ConnectionSettings>()!;
}