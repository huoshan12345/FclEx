namespace FclEx.RabbitMQ;

public class RabbitMQFixture : GlobalFixture
{
    public static ConnectionSettings RabbitMQConnection { get; } = Config.GetSection("RabbitMQ").Get<ConnectionSettings>()!;
}