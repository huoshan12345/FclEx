using System.IO;

namespace FclEx.RabbitMQ;

public class RabbitMqConsumerTests
{
    [Fact]
    public async Task NoncooperativeCallback_DoesNotHoldRunShutdownIndefinitely_OrAcknowledgeAfterClosure()
    {
        var doubles = new TestDoubles();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        doubles.Channel.Setup(c => c.DisposeAsync()).Returns(() => new ValueTask(release.Task));
        await using var runner = doubles.Runner<string>(async (_, _) =>
        {
            entered.TrySetResult(true);
            await release.Task;
            return MessageHandlingResult.Completed;
        }, shutdownTimeout: TimeSpan.FromMilliseconds(100));
        using var stop = new CancellationTokenSource();
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        var delivery = doubles.DeliverAsync();
        await entered.Task;
        stop.Cancel();
        try { await run.WaitAsync(TimeSpan.FromSeconds(2)); }
        finally { release.TrySetResult(true); await delivery; }
        doubles.Channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static TheoryData<bool, bool, bool> Failures => new()
    {
        { true, true, true }, { true, false, false }, { false, true, true },
    };

    [Theory]
    [MemberData(nameof(Failures))]
    public async Task HandlerFailure_RejectsOriginal_AndNeverAcknowledges(bool throws, bool retryEnabled, bool expectedRequeue)
    {
        var doubles = new TestDoubles();
        await using var runner = doubles.Runner<string>((_, _) => throws
            ? throw new InvalidOperationException("business failure") : Task.FromResult(MessageHandlingResult.Retry), retryEnabled);
        using var stop = new CancellationTokenSource();
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        await doubles.DeliverAsync();
        doubles.Channel.Verify(c => c.BasicRejectAsync(1, expectedRequeue, It.IsAny<CancellationToken>()), Times.Once);
        doubles.Channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        stop.Cancel();
        await run;
    }

    [Fact]
    public async Task InvalidSerialization_DeadLettersWithoutInvokingHandler()
    {
        var doubles = new TestDoubles();
        var invoked = false;
        await using var runner = doubles.Runner<int>((_, _) => { invoked = true; return Task.FromResult(MessageHandlingResult.Completed); });
        using var stop = new CancellationTokenSource();
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        await doubles.DeliverAsync("not-json");
        Assert.False(invoked);
        doubles.Channel.Verify(c => c.BasicRejectAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
        stop.Cancel();
        await run;
    }

    [Fact]
    public async Task UndefinedDisposition_IsFailure_NotAcknowledgement()
    {
        var doubles = new TestDoubles();
        await using var runner = doubles.Runner<string>((_, _) => Task.FromResult(default(MessageHandlingResult)));
        using var stop = new CancellationTokenSource();
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        await doubles.DeliverAsync();
        doubles.Channel.Verify(c => c.BasicRejectAsync(1, true, It.IsAny<CancellationToken>()), Times.Once);
        doubles.Channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        stop.Cancel();
        await run;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cancellation_AcknowledgesOnlyCompletedWork(bool completes)
    {
        var doubles = new TestDoubles();
        using var stop = new CancellationTokenSource();
        await using var runner = doubles.Runner<string>((_, token) =>
        {
            stop.Cancel();
            if (!completes) token.ThrowIfCancellationRequested();
            return Task.FromResult(MessageHandlingResult.Completed);
        });
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        await doubles.DeliverAsync();
        await run;
        doubles.Channel.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), completes ? Times.Once() : Times.Never());
        doubles.Channel.Verify(c => c.BasicRejectAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync(CancellationToken.None));
    }

    [Fact]
    public async Task AcknowledgementFailure_FaultsRun()
    {
        var doubles = new TestDoubles();
        var failure = new IOException("ack transport failed");
        doubles.Channel.Setup(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>())).Throws(failure);
        await using var runner = doubles.Runner<string>((_, _) => Task.FromResult(MessageHandlingResult.Completed));
        var run = runner.RunAsync(CancellationToken.None);
        await doubles.Subscribed.Task;
        await doubles.DeliverAsync();
        Assert.Same(failure, await Record.ExceptionAsync(() => run));
    }

    [Fact]
    public async Task ConnectionEpochChange_DoesNotAcknowledgeOldDeliveryOnRecoveredChannel()
    {
        var doubles = new TestDoubles(automaticRecovery: true);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var runner = doubles.Runner<string>(async (_, _) =>
        {
            entered.TrySetResult(true);
            await release.Task;
            return MessageHandlingResult.Completed;
        });
        using var stop = new CancellationTokenSource();
        var run = runner.RunAsync(stop.Token);
        await doubles.Subscribed.Task;
        var delivery = doubles.DeliverAsync();
        await entered.Task;
        doubles.Connection.SetupGet(c => c.IsOpen).Returns(false);
        await doubles.Channel.RaiseAsync(c => c.ChannelShutdownAsync += null, doubles.Channel.Object,
            new ShutdownEventArgs(ShutdownInitiator.Library, 541, "connection lost"));
        release.TrySetResult(true);
        await delivery;
        doubles.Channel.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), Times.Never);
        Assert.False(run.IsCompleted);
        doubles.Connection.SetupGet(c => c.IsOpen).Returns(true);
        await doubles.DeliverAsync(tag: 2);
        doubles.Channel.Verify(c => c.BasicAckAsync(2, false, It.IsAny<CancellationToken>()), Times.Once);
        stop.Cancel();
        await run;
    }
}
