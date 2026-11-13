// ReSharper disable AccessToDisposedClosure

namespace FclEx.Kafka;

public class ConsumerConfigExtensionsTests
{
    public static TheoryData<int> ErrorHandlerModes => new() { 0, 1, 2 };

    [Theory]
    [MemberData(nameof(ErrorHandlerModes))]
    public async Task FailedHandler_DoesNotCommitOrConsumeLaterMessages(int errorHandlerMode)
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync("success", "failure", "later");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var failure = new InvalidOperationException("handler failed");
        var handled = new List<string>();
        var errorHandlerCalls = 0;
        var options = CreateOptions(topic, value =>
        {
            handled.Add(value);
            if (value == "failure")
                throw failure;
            if (value == "later")
                timeout.Cancel();
            return Task.CompletedTask;
        }, timeout.Token);

        if (errorHandlerMode != 0)
        {
            options.ErrorHandler = (_, exception) =>
            {
                Assert.Same(failure, exception);
                errorHandlerCalls++;
                if (errorHandlerMode == 2)
                    throw new InvalidOperationException("error handler failed");
                return Task.CompletedTask;
            };
        }

        var exception = await Record.ExceptionAsync(() => StartConsumer(topic.CreateConsumerConfig(), options));

        Assert.Equal(new Offset(1), topic.GetCommittedOffset());
        Assert.Equal(new[] { "success", "failure" }, handled);
        Assert.Same(failure, exception);
        Assert.Equal(errorHandlerMode == 0 ? 0 : 1, errorHandlerCalls);
        Assert.Equal("failure", topic.ReplayNextMessage());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    public async Task InFlightHandler_DoesNotAutoCommit_AndCommitsAfterSuccess(bool? enableAutoCommit)
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync("message");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var config = topic.CreateConsumerConfig();
        config.EnableAutoCommit = enableAutoCommit;
        config.EnableAutoOffsetStore = enableAutoCommit;
        var originalConfig = config.ToArray();
        var task = StartConsumer(config, CreateOptions(topic, async _ =>
        {
            entered.TrySetResult(true);
            await release.Task;
            await timeout.CancelAsync();
        }, timeout.Token));
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(20), CoreTestsFixture.CancellationToken);
            // Span many automatic commit intervals while the handler has not completed.
            await Task.Delay(TimeSpan.FromSeconds(2), CoreTestsFixture.CancellationToken);
            Assert.Equal(Offset.Unset, topic.GetCommittedOffset());
        }
        finally
        {
            release.TrySetResult(true);
            await timeout.CancelAsync();
            await task.WaitAsync(TimeSpan.FromSeconds(15), CoreTestsFixture.CancellationToken);
        }

        Assert.Equal(new Offset(1), topic.GetCommittedOffset());
        Assert.Equal(originalConfig, config.ToArray());
    }

    [Fact]
    public async Task DeserializationFailure_DoesNotCommitPastInvalidMessage()
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync("invalid json", "\"later\"");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var handlerCalled = false;

        var options = CreateOptions(topic, _ =>
        {
            handlerCalled = true;
            return timeout.CancelAsync();
        }, timeout.Token);
        options.Deserializer = new JsonDeserializer<string>();

        var exception = await Record.ExceptionAsync(() => StartConsumer(topic.CreateConsumerConfig(), options));

        Assert.Equal(Offset.Unset, topic.GetCommittedOffset());
        Assert.False(handlerCalled);
        Assert.IsType<ConsumeException>(exception);
        Assert.Equal("invalid json", topic.ReplayNextMessage());
    }

    [Theory]
    [InlineData("value")]
    [InlineData(null)]
    public async Task SuccessfulHandler_CommitsMessageIncludingTombstones(string? value)
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync(value!);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var handled = false;

        await StartConsumer(topic.CreateConsumerConfig(), CreateOptions(topic, message =>
        {
            Assert.Equal(value, message);
            handled = true;
            timeout.Cancel();
            return Task.CompletedTask;
        }, timeout.Token));

        Assert.True(handled);
        Assert.Equal(new Offset(1), topic.GetCommittedOffset());
    }

    [Fact]
    public async Task CanceledHandler_DoesNotCommitOrConsumeLaterMessages()
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync("cancel", "later");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var handled = new List<string>();
        var errorHandlerCalled = false;
        var options = CreateOptions(topic, message =>
        {
            handled.Add(message);
            timeout.Cancel();
            timeout.Token.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }, timeout.Token);
        options.ErrorHandler = (_, _) =>
        {
            errorHandlerCalled = true;
            return Task.CompletedTask;
        };

        await StartConsumer(topic.CreateConsumerConfig(), options);

        Assert.Equal(new[] { "cancel" }, handled);
        Assert.False(errorHandlerCalled);
        Assert.Equal(Offset.Unset, topic.GetCommittedOffset());
        Assert.Equal("cancel", topic.ReplayNextMessage());
    }

    [Fact]
    public async Task PartitionEnd_AndIdleCancellation_DoNotHandleOrCommitControlRecords()
    {
        await using var topic = await KafkaTestsFixture.CreateTopicAsync();
        await topic.PublishAsync("message");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var config = topic.CreateConsumerConfig();
        config.EnablePartitionEof = true;
        var handled = new List<string>();
        await StartConsumer(config, CreateOptions(topic, message =>
        {
            handled.Add(message);
            // Allow the consumer to receive the partition EOF, then cancel its blocking poll.
            timeout.CancelAfter(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }, timeout.Token)).WaitAsync(TimeSpan.FromSeconds(20), CoreTestsFixture.CancellationToken);

        Assert.Equal(new[] { "message" }, handled);
        Assert.Equal(new Offset(1), topic.GetCommittedOffset());
    }

    private static KafkaConsumerOptions<string> CreateOptions(KafkaTestTopic topic, Func<string, Task> handler, CancellationToken token)
    {
        return new KafkaConsumerOptions<string>
        {
            Name = nameof(ConsumerConfigExtensionsTests),
            Topic = topic.Name,
            MessageHandler = handler,
            Deserializer = Deserializers.Utf8,
            CancellationToken = token,
        };
    }

    private static Task StartConsumer(ConsumerConfig config, KafkaConsumerOptions<string> options)
        => config.StartKafkaConsumer(options);
}
