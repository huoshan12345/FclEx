namespace FclEx.RabbitMQ;

public class RabbitMQTests(RabbitMQFixture fixture) : IAssemblyFixture<RabbitMQFixture>
{
    public RabbitMQFixture Fixture { get; } = fixture;
}