namespace FclEx.RabbitMQ;

public class RabbitMQPublisherTests
{
    public static TheoryData<object?, string, string> DefaultFormats => new()
    {
        { "text", "text/plain", "text" },
        { 42, "application/json", "42" },
        { null, "application/json", "null" },
    };

    [Theory]
    [MemberData(nameof(DefaultFormats))]
    public async Task DefaultSerializer_ContentTypeMatchesActualValue_EvenForObjectPublisher(object? value, string contentType, string body)
    {
        var doubles = new TestDoubles();
        BasicProperties? captured = null;
        ReadOnlyMemory<byte> capturedBody = default;
        doubles.Channel.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), true,
                It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, string _, bool _, BasicProperties properties, ReadOnlyMemory<byte> bytes, CancellationToken _) =>
            {
                captured = properties;
                capturedBody = bytes.ToArray();
            });
        await using var publisher = new RabbitMQPublisher<object?>(doubles.Client, doubles.Channel.Object, new() { Exchange = RabbitMQExchangeOptions.Default });
        await publisher.PublishAsync(new(value));
        Assert.Equal(contentType, captured!.ContentType);
        Assert.Equal(body, System.Text.Encoding.UTF8.GetString(capturedBody.ToArray()));
    }

    [Fact]
    public async Task Publication_WaitsForConfirmation_AndSerializesCalls()
    {
        var doubles = new TestDoubles();
        var confirm = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        BasicProperties? captured = null;
        doubles.Channel.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), true,
                It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
            .Returns((string _, string _, bool _, BasicProperties properties, ReadOnlyMemory<byte> _, CancellationToken _) =>
            {
                captured = properties;
                Interlocked.Increment(ref calls);
                entered.TrySetResult(true);
                return new ValueTask(confirm.Task);
            });
        await using var publisher = new RabbitMQPublisher<string>(doubles.Client, doubles.Channel.Object,
            new() { Exchange = RabbitMQExchangeOptions.Default });
        var first = publisher.PublishAsync(new("first", new() { RoutingKey = "queue" }, messageId: "stable-id"));
        await entered.Task;
        var second = publisher.PublishAsync(new("second"));
        Assert.False(first.IsCompleted);
        Assert.Equal(1, calls);
        Assert.True(captured!.Persistent);
        Assert.Equal("stable-id", captured.MessageId);
        confirm.SetResult(true);
        await Task.WhenAll(first, second);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task Timeout_AndCallerCancellation_AreDistinct()
    {
        var doubles = new TestDoubles();
        doubles.Channel.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), true,
                It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
            .Returns((string _, string _, bool _, BasicProperties _, ReadOnlyMemory<byte> _, CancellationToken token)
                => new ValueTask(Task.Delay(Timeout.Infinite, token)));
        await using var publisher = new RabbitMQPublisher<string>(doubles.Client, doubles.Channel.Object,
            new() { Exchange = RabbitMQExchangeOptions.Default, ConfirmationTimeout = TimeSpan.FromMilliseconds(50) });
        await Assert.ThrowsAsync<TimeoutException>(() => publisher.PublishAsync(new("body")));
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => publisher.PublishAsync(new("body"), cancel.Token));
    }

    [Fact]
    public async Task Delay_Requires44_AndUsesInt64AbsoluteTimestamp()
    {
        var old = new TestDoubles("4.3.6");
        await using var oldPublisher = new RabbitMQPublisher<string>(old.Client, old.Channel.Object, new() { Exchange = RabbitMQExchangeOptions.Default });
        await Assert.ThrowsAsync<NotSupportedException>(() => oldPublisher.PublishAsync(new("delayed", new() { Delay = TimeSpan.FromSeconds(1) })));
        await oldPublisher.PublishAsync(new("immediate"));
        var current = new TestDoubles();
        var at = DateTimeOffset.UtcNow.AddMinutes(1);
        BasicProperties? properties = null;
        current.Channel.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), true,
                It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
            .Callback((string _, string _, bool _, BasicProperties p, ReadOnlyMemory<byte> _, CancellationToken _) => properties = p);
        await using var publisher = new RabbitMQPublisher<string>(current.Client, current.Channel.Object, new() { Exchange = RabbitMQExchangeOptions.Default });
        await publisher.PublishAsync(new("scheduled", new() { DeliverAt = at }));
        Assert.Equal(at.ToUnixTimeMilliseconds() + (at.Ticks % TimeSpan.TicksPerMillisecond == 0 ? 0 : 1), Assert.IsType<long>(properties!.Headers!["x-opt-delivery-time"]));
    }

    [Fact]
    public async Task DisposedPublisher_RejectsNewPublications()
    {
        var doubles = new TestDoubles();
        var publisher = new RabbitMQPublisher<string>(doubles.Client, doubles.Channel.Object, new() { Exchange = RabbitMQExchangeOptions.Default });
        await publisher.DisposeAsync();
        await publisher.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => publisher.PublishAsync(new("body")));
        doubles.Channel.Verify(c => c.DisposeAsync(), Times.Once);
    }
}
