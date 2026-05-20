namespace FclEx.RabbitMQ;

public class RabbitMQFixture : GlobalFixture
{
    public static RabbitMqConnectionOptions ConnectionSettings { get; } = Config.GetSection("RabbitMQ").Get<RabbitMqConnectionOptions>()!;
}