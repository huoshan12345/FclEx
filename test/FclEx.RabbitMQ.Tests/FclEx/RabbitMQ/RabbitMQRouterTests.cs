namespace FclEx.RabbitMQ;

public class RabbitMQRouterTests
{
    [Fact]
    public async Task NullOutputOptions_AreNotReplacedBySilentDefaultRouting()
    {
        var input = new TestDoubles();
        var output = new Mock<IChannel>();
        var options = OptionsSnapshot.Router(new()
        {
            Input = new() { Queue = new() { Name = "input" } },
            Target = new() { Exchange = RabbitMQExchangeOptions.Default },
        });
        var publisher = new RabbitMQPublisher<string>(input.Client, output.Object, options.Target);
        await using var router = new RabbitMQRouter<string, string>(input.Client, input.Channel.Object, options,
            new MessageConverter<string, string>((message, _) => Task.FromResult(message.Body)), publisher, (_, _) => null!);
        using var stop = new CancellationTokenSource();
        var run = router.RunAsync(stop.Token);
        await input.Subscribed.Task;
        await input.DeliverAsync();
        output.Verify(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>(),
            It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Never);
        input.Channel.Verify(c => c.BasicRejectAsync(1, true, It.IsAny<CancellationToken>()), Times.Once);
        input.Channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        stop.Cancel();
        await run;
    }

    [Fact]
    public async Task InputAcknowledgement_WaitsForOutputConfirmation()
    {
        var input = new TestDoubles();
        var output = new Mock<IChannel>();
        var confirmed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var published = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        output.Setup(c => c.BasicPublishAsync(It.IsAny<string>(), It.IsAny<string>(), true,
                It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()))
            .Returns((string _, string _, bool _, BasicProperties _, ReadOnlyMemory<byte> _, CancellationToken _) =>
            {
                published.TrySetResult(true);
                return new ValueTask(confirmed.Task);
            });
        var options = OptionsSnapshot.Router(new()
        {
            Input = new() { Queue = new() { Name = "input" } },
            Target = new() { Exchange = RabbitMQExchangeOptions.Default },
        });
        var publisher = new RabbitMQPublisher<string>(input.Client, output.Object, options.Target);
        await using var router = new RabbitMQRouter<string, string>(input.Client, input.Channel.Object, options,
            new MessageConverter<string, string>((message, _) => Task.FromResult(message.Body)), publisher, null);
        using var stop = new CancellationTokenSource();
        var run = router.RunAsync(stop.Token);
        await input.Subscribed.Task;
        var delivery = input.DeliverAsync();
        await published.Task;
        input.Channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        confirmed.SetResult(true);
        await delivery;
        input.Channel.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
        stop.Cancel();
        await run;
    }
}
