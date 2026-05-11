using FclEx.Xunit;

namespace FclEx.RabbitMQ;

[CollectionDefinition(nameof(RabbitMQTestsCollection))]
public class RabbitMQTestsCollection : ICollectionFixture<RabbitMQFixture>;

[Collection(nameof(RabbitMQTestsCollection))]
public class RabbitMQTests(RabbitMQFixture fixture)
{
    public RabbitMQFixture Fixture { get; } = fixture;

    // there is no RabbitMQ server in GitHub Action Windows runner.
    public static bool Skip => TestHelper.IsGithubAction && TestHelper.IsWindows;
}