using Microsoft.Extensions.Logging;

namespace FclEx.RabbitMQ;

public class RabbitMQClientTests
{
    [Fact]
    public async Task RouterConstructionFailure_ReleasesBothPreparedChannels()
    {
        var doubles = new TestDoubles();
        var output = new Mock<IChannel>();
        var channels = new Queue<IChannel>([output.Object, doubles.Channel.Object]);
        doubles.Connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => channels.Dequeue());
        var logging = new Mock<ILoggerFactory>();
        logging.SetupSequence(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(NullLogger.Instance).Throws(new InvalidOperationException("consumer logger initialization failed"));
        await using var client = new RabbitMQClient(doubles.Connection.Object, SerializerPresets.Utf8StringOrJson, logging.Object, false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.CreateRouterAsync(new RabbitMQRouterOptions
        {
            Input = new() { Queue = new() { Name = "input" } },
            Target = new() { Exchange = RabbitMQExchangeOptions.Default },
        }, new MessageConverter<string, string>((message, _) => Task.FromResult(message.Body))));
        doubles.Channel.Verify(c => c.DisposeAsync(), Times.Once);
        output.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task ConsumerConstructionFailure_ReleasesPreparedChannel()
    {
        var doubles = new TestDoubles();
        doubles.Connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(doubles.Channel.Object);
        var logging = new Mock<ILoggerFactory>();
        logging.Setup(f => f.CreateLogger(It.IsAny<string>())).Throws(new InvalidOperationException("logger initialization failed"));
        await using var client = new RabbitMQClient(doubles.Connection.Object, SerializerPresets.Utf8StringOrJson, logging.Object, false);
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.CreateConsumerAsync<string>(new() { Queue = new() { Name = "input" } },
            (_, _) => Task.FromResult(MessageHandlingResult.Completed)));
        doubles.Channel.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task PublisherCreation_EnablesTrackedConfirms_AndSupportsNormalLoggerFactory()
    {
        var doubles = new TestDoubles();
        CreateChannelOptions? created = null;
        doubles.Connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .Callback((CreateChannelOptions options, CancellationToken _) => created = options)
            .ReturnsAsync(doubles.Channel.Object);
        using var logging = LoggerFactory.Create(_ => { });
        await using var client = new RabbitMQClient(doubles.Connection.Object, SerializerPresets.Utf8StringOrJson, logging, false);
        await using var publisher = await client.CreatePublisherAsync<string>(new() { Exchange = new() { Name = "events" } });
        await publisher.PublishAsync(new("message"));
        Assert.True(created!.PublisherConfirmationsEnabled);
        Assert.True(created.PublisherConfirmationTrackingEnabled);
    }

    [Fact]
    public async Task ClientDisposal_CleansEveryRoleAndConnection_EvenAfterChannelFailure()
    {
        var doubles = new TestDoubles();
        var other = new Mock<IChannel>();
        var channels = new Queue<IChannel>([doubles.Channel.Object, other.Object]);
        doubles.Connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => channels.Dequeue());
        other.Setup(c => c.DisposeAsync()).Throws(new InvalidOperationException("cleanup failed"));
        var first = await doubles.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMQExchangeOptions.Default });
        await doubles.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMQExchangeOptions.Default });
        await Assert.ThrowsAsync<AggregateException>(() => doubles.Client.DisposeAsync().AsTask());
        doubles.Channel.Verify(c => c.DisposeAsync(), Times.Once);
        doubles.Connection.Verify(c => c.DisposeAsync(), Times.Once);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => first.PublishAsync(new("body")));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => doubles.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMQExchangeOptions.Default }));
    }

    [Fact]
    public async Task FailedTopologyCreation_ReleasesTheNewChannel_AndKeepsConnectionOwnedByClient()
    {
        var doubles = new TestDoubles();
        doubles.Connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(doubles.Channel.Object);
        var failure = new InvalidOperationException("declaration mismatch");
        doubles.Channel.Setup(c => c.ExchangeDeclareAsync("conflict", It.IsAny<string>(), true, false,
            It.IsAny<IDictionary<string, object?>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).Throws(failure);
        Assert.Same(failure, await Record.ExceptionAsync(() => doubles.Client.CreatePublisherAsync<string>(new() { Exchange = new() { Name = "conflict" } })));
        doubles.Channel.Verify(c => c.DisposeAsync(), Times.Once);
        doubles.Connection.Verify(c => c.DisposeAsync(), Times.Never);
        await doubles.Client.DisposeAsync();
        doubles.Connection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Theory]
    [InlineData("3.13.0")]
    [InlineData("4.2.9")]
    [InlineData("unknown")]
    public void UnsupportedOrUnknownBrokerVersion_IsNotSilentlyAccepted(string version)
    {
        var doubles = new TestDoubles(version);
        Assert.Throws<NotSupportedException>(() => doubles.Client.RequireVersion(4, 3));
    }
}
