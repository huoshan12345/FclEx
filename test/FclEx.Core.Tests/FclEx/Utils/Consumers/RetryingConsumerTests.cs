using System.Collections.Concurrent;

namespace FclEx.Utils.Consumers;

public class RetryingConsumerTests
{
    [Fact]
    public async Task Failed_Item_Is_Retried_Then_Discarded()
    {
        await using var consumer = new RetryingConsumer<int>(
            (_, _) => throw new InvalidOperationException("failed"),
            maxRetryCount: 2);
        var failures = new List<ItemConsumptionFailure<int>>();
        var discarded = new List<DiscardedItem<int>>();
        consumer.ItemFailed += (_, failure) => failures.Add(failure);
        consumer.ItemDiscarded += (_, item) => discarded.Add(item);

        consumer.Enqueue(42);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Equal([1, 2, 3], failures.Select(x => x.AttemptNumber));
        Assert.Equal(
            [ConsumerFailureAction.Retry, ConsumerFailureAction.Retry, ConsumerFailureAction.Discard],
            failures.Select(x => x.Action));
        var discardedItem = Assert.Single(discarded);
        Assert.Equal(42, discardedItem.Item);
        Assert.Equal(3, discardedItem.AttemptCount);
        Assert.Equal(0, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(3, consumer.Metrics.FailedConsumptionCount);
        Assert.Equal(1, consumer.Metrics.DiscardedItemCount);
        Assert.True(consumer.IsCompleted);
    }

    [Fact]
    public async Task Retry_Is_Queued_Behind_Already_Pending_Items()
    {
        var attempts = new Dictionary<int, int>();
        var invocationOrder = new List<int>();
        await using var consumer = new RetryingConsumer<int>((item, _) =>
        {
            invocationOrder.Add(item);
            attempts.TryGetValue(item, out var attemptCount);
            attempts[item] = attemptCount + 1;
            return item == 1 && attempts[item] == 1
                ? Task.FromException(new InvalidOperationException("retry"))
                : Task.CompletedTask;
        });

        consumer.EnqueueRange([1, 2]);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Equal([1, 2, 1], invocationOrder);
        Assert.Equal(2, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(1, consumer.Metrics.FailedConsumptionCount);
    }

    [Fact]
    public async Task Listener_Exception_Does_Not_Change_Consumption_State()
    {
        await using var consumer = new RetryingConsumer<int>((_, _) => Task.CompletedTask);
        var listenerFailures = new List<ConsumerListenerFailure>();
        consumer.ItemConsumed += (_, _) => throw new InvalidOperationException("listener");
        consumer.ListenerFailed += (_, failure) => listenerFailures.Add(failure);

        consumer.Enqueue(1);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        var failure = Assert.Single(listenerFailures);
        Assert.Equal(nameof(consumer.ItemConsumed), failure.NotificationName);
        Assert.IsType<InvalidOperationException>(failure.Exception);
        Assert.Equal(1, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(0, consumer.Metrics.FailedConsumptionCount);
    }

    [Fact]
    public async Task Stop_Waits_For_Active_Consumption_And_Abandons_Pending_Items()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var abandoned = new List<int>();
        await using var consumer = new RetryingConsumer<int>(async (_, _) =>
        {
            entered.TrySetResult(true);
            await release.Task;
        });
        consumer.ItemsAbandoned += (_, items) => abandoned.AddRange(items);
        consumer.EnqueueRange([1, 2]);

        var runTask = consumer.StartAsync();
        await CompletesWithin(entered.Task);
        var stopTask = consumer.StopAsync();
        await Task.Delay(50);
        Assert.False(stopTask.IsCompleted);

        release.SetResult(true);
        await CompletesWithin(stopTask);
        await CompletesWithin(runTask);

        Assert.Equal([2], abandoned);
        Assert.Equal(1, consumer.Metrics.ConsumedItemCount);
        Assert.True(consumer.IsCompleted);
    }

    [Fact]
    public async Task Enqueue_And_CompleteAdding_Are_Atomic()
    {
        var consumed = new ConcurrentBag<int>();
        await using var consumer = new RetryingConsumer<int>((item, _) =>
        {
            consumed.Add(item);
            return Task.CompletedTask;
        });
        var accepted = new ConcurrentBag<int>();
        var runTask = consumer.StartAsync();
        using var start = new ManualResetEventSlim();
        var producers = Enumerable.Range(0, 4).Select(producer => Task.Run(() =>
        {
            start.Wait();
            for (var i = 0; i < 250; i++)
            {
                var item = producer * 250 + i;
                try
                {
                    consumer.Enqueue(item);
                    accepted.Add(item);
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
        })).ToArray();
        var completeTask = Task.Run(() =>
        {
            start.Wait();
            consumer.CompleteAdding();
        });

        start.Set();
        await Task.WhenAll(producers.Append(completeTask));
        await CompletesWithin(runTask);

        Assert.Equal(accepted.OrderBy(x => x), consumed.OrderBy(x => x));
    }

    [Fact]
    public async Task Consumer_Can_Only_Be_Started_Once()
    {
        await using var consumer = new RetryingConsumer<int>((_, _) => Task.CompletedTask);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = consumer.StartAsync();
        });
    }

    [Fact]
    public async Task Zero_Retries_Discards_After_Initial_Attempt()
    {
        var attempts = 0;
        ItemConsumptionFailure<int>? failure = null;
        await using var consumer = new RetryingConsumer<int>(
            (_, _) =>
            {
                attempts++;
                throw new InvalidOperationException("failed");
            },
            maxRetryCount: 0);
        consumer.ItemFailed += (_, value) => failure = value;

        consumer.Enqueue(1);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Equal(1, attempts);
        Assert.NotNull(failure);
        Assert.Equal(1, failure.AttemptNumber);
        Assert.Equal(ConsumerFailureAction.Discard, failure.Action);
        Assert.Equal(1, consumer.Metrics.DiscardedItemCount);
    }

    [Fact]
    public async Task Cancellation_Abandons_The_Active_Item()
    {
        using var cancellation = new CancellationTokenSource();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var abandoned = new List<int>();
        await using var consumer = new RetryingConsumer<int>(async (_, cancellationToken) =>
        {
            entered.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken);
        });
        consumer.ItemsAbandoned += (_, items) => abandoned.AddRange(items);
        consumer.Enqueue(7);

        var runTask = consumer.StartAsync(cancellation.Token);
        await CompletesWithin(entered.Task);
        cancellation.Cancel();
        await CompletesWithin(runTask);

        Assert.Equal([7], abandoned);
        Assert.Equal(0, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(0, consumer.Metrics.FailedConsumptionCount);
        Assert.True(consumer.IsCompleted);
    }

    [Fact]
    public async Task Stop_Before_Start_Abandons_All_Items_And_Closes_The_Consumer()
    {
        var abandoned = new List<int>();
        await using var consumer = new RetryingConsumer<int>((_, _) => Task.CompletedTask);
        consumer.ItemsAbandoned += (_, items) => abandoned.AddRange(items);
        consumer.EnqueueRange([1, 2]);

        await consumer.StopAsync();

        Assert.Equal([1, 2], abandoned);
        Assert.Equal(0, consumer.PendingCount);
        Assert.True(consumer.IsAddingCompleted);
        Assert.True(consumer.IsCompleted);
        Assert.Throws<InvalidOperationException>(() => consumer.Enqueue(3));
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = consumer.StartAsync();
        });
    }

    private static async Task CompletesWithin(Task task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(task, completed);
        await task;
    }
}
