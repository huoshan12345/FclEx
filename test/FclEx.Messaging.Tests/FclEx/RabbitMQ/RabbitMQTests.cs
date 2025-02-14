namespace FclEx.RabbitMQ;

public class RabbitMQTests(RabbitMQFixture fixture) : IClassFixture<RabbitMQFixture>
{
    public RabbitMQFixture Fixture { get; } = fixture;
}