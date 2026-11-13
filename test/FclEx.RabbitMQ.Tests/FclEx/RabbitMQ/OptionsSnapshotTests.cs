namespace FclEx.RabbitMQ;

public class OptionsSnapshotTests
{
    public static TheoryData<RabbitMQQueueOptions> InvalidQueues => new()
    {
        new RabbitMQQueueOptions() { Name = "" },
        new RabbitMQQueueOptions() { Name = "amq.system" },
        new RabbitMQQueueOptions() { Name = new string('界', 86) },
        new RabbitMQQueueOptions() { Name = "input", DeadLetter = new() { QueueName = "input" } },
        new RabbitMQQueueOptions() { Name = "input", MaxLength = 0 },
        new RabbitMQQueueOptions() { Name = "input", Retry = new() { MinDelay = TimeSpan.Zero } },
        new RabbitMQQueueOptions() { Name = "input", Retry = new() { MinDelay = TimeSpan.FromSeconds(2), MaxDelay = TimeSpan.FromSeconds(1) } },
        new RabbitMQQueueOptions() { Name = "input", Retry = new() { DeliveryLimit = 0 } },
        new RabbitMQQueueOptions() { Name = "input", Arguments = new Dictionary<string, object?> { ["x-queue-type"] = "classic" } },
    };

    [Theory]
    [MemberData(nameof(InvalidQueues))]
    public void InvalidOrConflictingQueueContracts_AreRejectedBeforeDeclaration(RabbitMQQueueOptions options)
        => Assert.ThrowsAny<ArgumentException>(() => OptionsSnapshot.Queue(options));

    [Fact]
    public void ConsumerSnapshot_DetachesBindingsAndArguments_AndRejectsDefaultExchangeBindings()
    {
        var bytes = new byte[] { 1 };
        var bindings = new[] { new RabbitMQBindingOptions { Exchange = new() { Name = "events" }, Arguments = new Dictionary<string, object?> { ["custom"] = bytes } } };
        var snapshot = OptionsSnapshot.Consumer(new() { Queue = new() { Name = "input" }, Bindings = bindings });
        bytes[0] = 2;
        bindings[0] = new() { Exchange = new() { Name = "changed" } };
        Assert.Equal("events", snapshot.Bindings[0].Exchange.Name);
        Assert.Equal(new byte[] { 1 }, snapshot.Bindings[0].Arguments["custom"]);
        Assert.Throws<ArgumentException>(() => OptionsSnapshot.Binding(new() { Exchange = RabbitMQExchangeOptions.Default }));
        Assert.Throws<ArgumentException>(() => OptionsSnapshot.Consumer(new() { Queue = new() { Name = "input" }, PrefetchCount = 1, MaxConcurrency = 2 }));
    }
}
