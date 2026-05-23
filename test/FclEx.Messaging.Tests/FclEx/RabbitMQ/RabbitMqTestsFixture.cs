namespace FclEx.RabbitMQ;

public class RabbitMqTestsFixture : CoreTestsFixture
{
    public static RabbitMqConnectionOptions ConnectionSettings { get; } = Config.GetSection("RabbitMQ").Get<RabbitMqConnectionOptions>()!;
}