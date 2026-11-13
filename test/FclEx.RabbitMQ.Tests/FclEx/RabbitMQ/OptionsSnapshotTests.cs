namespace FclEx.RabbitMQ;

public class OptionsSnapshotTests
{
    public static TheoryData<RabbitMqQueueOptions> InvalidQueues => new()
    {
        new RabbitMqQueueOptions() { Name = "" },
        new RabbitMqQueueOptions() { Name = "amq.system" },
        new RabbitMqQueueOptions() { Name = new string('界', 86) },
        new RabbitMqQueueOptions() { Name = "input", DeadLetter = new() { QueueName = "input" } },
        new RabbitMqQueueOptions() { Name = "input", MaxLength = 0 },
        new RabbitMqQueueOptions() { Name = "input", Retry = new() { MinDelay = TimeSpan.Zero } },
        new RabbitMqQueueOptions() { Name = "input", Retry = new() { MinDelay = TimeSpan.FromSeconds(2), MaxDelay = TimeSpan.FromSeconds(1) } },
        new RabbitMqQueueOptions() { Name = "input", Retry = new() { DeliveryLimit = 0 } },
        new RabbitMqQueueOptions() { Name = "input", Arguments = new Dictionary<string, object?> { ["x-queue-type"] = "classic" } },
    };

    [Theory]
    [MemberData(nameof(InvalidQueues))]
    public void InvalidOrConflictingQueueContracts_AreRejectedBeforeDeclaration(RabbitMqQueueOptions options)
        => Assert.ThrowsAny<ArgumentException>(() => OptionsSnapshot.Queue(options));

    [Fact]
    public void ConsumerSnapshot_DetachesBindingsAndArguments_AndRejectsDefaultExchangeBindings()
    {
        var bytes = new byte[] { 1 };
        var bindings = new[] { new RabbitMqBindingOptions { Exchange = new() { Name = "events" }, Arguments = new Dictionary<string, object?> { ["custom"] = bytes } } };
        var snapshot = OptionsSnapshot.Consumer(new() { Queue = new() { Name = "input" }, Bindings = bindings });
        bytes[0] = 2;
        bindings[0] = new() { Exchange = new() { Name = "changed" } };
        Assert.Equal("events", snapshot.Bindings[0].Exchange.Name);
        Assert.Equal(new byte[] { 1 }, snapshot.Bindings[0].Arguments["custom"]);
        Assert.Throws<ArgumentException>(() => OptionsSnapshot.Binding(new() { Exchange = RabbitMqExchangeOptions.Default }));
        Assert.Throws<ArgumentException>(() => OptionsSnapshot.Consumer(new() { Queue = new() { Name = "input" }, PrefetchCount = 1, MaxConcurrency = 2 }));
    }
}
