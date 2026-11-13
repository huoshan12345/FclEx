namespace RabbitMQ.Client;

public class RabbitMqChannelExtensionsTests
{
    [Fact]
    public async Task QuorumTopology_UsesReliableDeadLettering_AndDeclaresTargetsFirst()
    {
        var channel = new Mock<IChannel>();
        var events = new List<string>();
        Dictionary<string, object?>? sourceArguments = null;
        channel.Setup(c => c.ExchangeDeclareAsync(It.IsAny<string>(), It.IsAny<string>(), true, false,
                It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback((string name, string _, bool _, bool _, IDictionary<string, object?> _, bool _, bool _, CancellationToken _) => events.Add(name));
        channel.Setup(c => c.QueueDeclareAsync(It.IsAny<string>(), true, false, false,
                It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback((string name, bool _, bool _, bool _, IDictionary<string, object?> arguments, bool _, bool _, CancellationToken _) =>
            {
                events.Add(name);
                if (name == "input") sourceArguments = new(arguments);
            });
        await channel.Object.EnsureQueueAsync(new FclEx.RabbitMQ.RabbitMqQueueOptions { Name = "input" });
        Assert.Equal(new[] { "input.dlx", "input.dead", "input" }, events);
        Assert.Equal("quorum", sourceArguments!["x-queue-type"]);
        Assert.Equal("at-least-once", sourceArguments["x-dead-letter-strategy"]);
        Assert.Equal("reject-publish", sourceArguments["x-overflow"]);
        Assert.Equal("input.dlx", sourceArguments["x-dead-letter-exchange"]);
        Assert.Equal("all", sourceArguments["x-delayed-retry-type"]);
    }
}
