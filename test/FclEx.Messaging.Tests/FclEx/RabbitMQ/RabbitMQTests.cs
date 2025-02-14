namespace FclEx.RabbitMQ;

[CollectionDefinition(nameof(RabbitMQTestsCollection))]
public class RabbitMQTestsCollection : ICollectionFixture<RabbitMQFixture>;

[Collection(nameof(RabbitMQTestsCollection))]
public class RabbitMQTests(RabbitMQFixture fixture)
{
    public RabbitMQFixture Fixture { get; } = fixture;
}