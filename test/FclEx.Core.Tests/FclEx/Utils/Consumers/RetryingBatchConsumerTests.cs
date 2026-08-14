namespace FclEx.Utils.Consumers;

public class RetryingBatchConsumerTests
{
    [Fact]
    public async Task Full_Batch_Is_Consumed_Immediately()
    {
        var consumed = new TaskCompletionSource<IReadOnlyList<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = new RetryingBatchConsumer<int>(
            (items, _) =>
            {
                consumed.TrySetResult(items.ToArray());
                return Task.CompletedTask;
            },
            batchSize: 3,
            maxBatchInterval: TimeSpan.FromSeconds(10));

        var runTask = consumer.StartAsync();
        consumer.EnqueueRange([1, 2, 3]);
        await CompletesWithin(consumed.Task);
        consumer.CompleteAdding();
        await CompletesWithin(runTask);

        Assert.True(consumed.Task.IsCompleted);
        Assert.Equal([1, 2, 3], await consumed.Task);
        Assert.Equal(3, consumer.Metrics.ConsumedItemCount);
    }

    [Fact]
    public async Task Empty_Queue_Does_Not_Trigger_Consumption_And_Overdue_Item_Is_Immediate()
    {
        var calls = 0;
        var consumed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = new RetryingBatchConsumer<int>(
            (_, _) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                Interlocked.Increment(ref calls);
                consumed.TrySetResult(true);
                return Task.CompletedTask;
            },
            batchSize: 10,
            maxBatchInterval: TimeSpan.FromMilliseconds(100));

        var runTask = consumer.StartAsync();
        await Task.Delay(250);
        Assert.Equal(0, Volatile.Read(ref calls));

        consumer.Enqueue(1);
        await CompletesWithin(consumed.Task);
        consumer.CompleteAdding();
        await CompletesWithin(runTask);

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task CompleteAdding_Flushes_Partial_Batch()
    {
        IReadOnlyList<int>? consumed = null;
        await using var consumer = new RetryingBatchConsumer<int>(
            (items, _) =>
            {
                consumed = items.ToArray();
                return Task.CompletedTask;
            },
            batchSize: 10,
            maxBatchInterval: TimeSpan.FromMinutes(1));

        consumer.EnqueueRange([1, 2]);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Equal([1, 2], consumed);
        Assert.True(consumer.IsCompleted);
    }

    [Fact]
    public async Task Partial_Batch_Is_Consumed_When_Maximum_Interval_Elapses()
    {
        var consumed = new TaskCompletionSource<IReadOnlyList<int>>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = new RetryingBatchConsumer<int>(
            (items, _) =>
            {
                consumed.TrySetResult(items.ToArray());
                return Task.CompletedTask;
            },
            batchSize: 10,
            maxBatchInterval: TimeSpan.FromMilliseconds(50));

        var runTask = consumer.StartAsync();
        consumer.Enqueue(1);
        await CompletesWithin(consumed.Task);
        consumer.CompleteAdding();
        await CompletesWithin(runTask);

        Assert.True(consumed.Task.IsCompleted);
        Assert.Equal([1], await consumed.Task);
    }

    [Fact]
    public async Task Failed_Batches_Are_Recursively_Split_And_Only_Bad_Item_Is_Discarded()
    {
        var invocations = new List<int[]>();
        var failures = new List<BatchConsumptionFailure<int>>();
        var discarded = new List<DiscardedBatchItem<int>>();
        var activeCalls = 0;
        var maximumActiveCalls = 0;
        await using var consumer = new RetryingBatchConsumer<int>(
            async (items, _) =>
            {
                var active = Interlocked.Increment(ref activeCalls);
                maximumActiveCalls = Math.Max(maximumActiveCalls, active);
                try
                {
                    invocations.Add(items.ToArray());
                    await Task.Yield();
                    if (items.Contains(3))
                        throw new InvalidOperationException("bad row");
                }
                finally
                {
                    Interlocked.Decrement(ref activeCalls);
                }
            },
            batchSize: 4,
            maxBatchInterval: TimeSpan.FromMinutes(1),
            maxRetryCount: 2,
            retryPartitionCount: 2);
        consumer.BatchFailed += (_, failure) => failures.Add(failure);
        consumer.ItemDiscarded += (_, item) => discarded.Add(item);

        consumer.EnqueueRange([1, 2, 3, 4]);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.Equal(
            [
                new[] { 1, 2, 3, 4 },
                new[] { 1, 2 },
                new[] { 3, 4 },
                new[] { 3 },
                new[] { 4 },
                new[] { 3 },
                new[] { 3 },
            ],
            invocations);
        Assert.Equal(
            [
                ConsumerFailureAction.Split,
                ConsumerFailureAction.Split,
                ConsumerFailureAction.Retry,
                ConsumerFailureAction.Retry,
                ConsumerFailureAction.Discard,
            ],
            failures.Select(x => x.Action));
        var discardedItem = Assert.Single(discarded);
        Assert.Equal(3, discardedItem.Item);
        Assert.Equal(2, discardedItem.RetryCount);
        Assert.Equal(1, maximumActiveCalls);
        Assert.Equal(3, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(5, consumer.Metrics.FailedConsumptionCount);
        Assert.Equal(1, consumer.Metrics.DiscardedItemCount);
    }

    [Fact]
    public async Task Retry_Segments_Are_Consumed_Before_New_Producer_Batches()
    {
        var initialBatchEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseInitialBatch = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var invocations = new List<int[]>();
        await using var consumer = new RetryingBatchConsumer<int>(
            async (items, _) =>
            {
                var invocation = items.ToArray();
                invocations.Add(invocation);
                if (invocations.Count == 1)
                {
                    initialBatchEntered.TrySetResult(true);
                    await releaseInitialBatch.Task;
                    throw new InvalidOperationException("split this batch");
                }
            },
            batchSize: 2,
            maxBatchInterval: TimeSpan.FromMinutes(1),
            retryPartitionCount: 2);
        consumer.EnqueueRange([1, 2]);

        var runTask = consumer.StartAsync();
        await CompletesWithin(initialBatchEntered.Task);
        consumer.EnqueueRange([3, 4]);
        consumer.CompleteAdding();
        releaseInitialBatch.SetResult(true);
        await CompletesWithin(runTask);

        Assert.Equal(
            [new[] { 1, 2 }, new[] { 1 }, new[] { 2 }, new[] { 3, 4 }],
            invocations);
    }

    [Fact]
    public async Task Zero_Retries_Discards_A_Failed_Singleton_Immediately()
    {
        BatchConsumptionFailure<int>? failure = null;
        DiscardedBatchItem<int>? discarded = null;
        await using var consumer = new RetryingBatchConsumer<int>(
            (_, _) => throw new InvalidOperationException("failed"),
            batchSize: 1,
            maxBatchInterval: TimeSpan.FromMinutes(1),
            maxRetryCount: 0);
        consumer.BatchFailed += (_, value) => failure = value;
        consumer.ItemDiscarded += (_, value) => discarded = value;

        consumer.Enqueue(1);
        consumer.CompleteAdding();
        await consumer.StartAsync();

        Assert.NotNull(failure);
        Assert.Equal(0, failure.SingletonRetryCount);
        Assert.Equal(ConsumerFailureAction.Discard, failure.Action);
        Assert.NotNull(discarded);
        Assert.Equal(1, discarded.Item);
        Assert.Equal(0, discarded.RetryCount);
        Assert.Equal(1, consumer.Metrics.FailedConsumptionCount);
        Assert.Equal(1, consumer.Metrics.DiscardedItemCount);
    }

    [Fact]
    public async Task Cancellation_Abandons_The_Active_Segment_And_Pending_Items()
    {
        using var cancellation = new CancellationTokenSource();
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var abandoned = new List<int>();
        await using var consumer = new RetryingBatchConsumer<int>(
            async (_, cancellationToken) =>
            {
                entered.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
            },
            batchSize: 1,
            maxBatchInterval: TimeSpan.FromMinutes(1));
        consumer.ItemsAbandoned += (_, items) => abandoned.AddRange(items);
        consumer.EnqueueRange([1, 2]);

        var runTask = consumer.StartAsync(cancellation.Token);
        await CompletesWithin(entered.Task);
        cancellation.Cancel();
        await CompletesWithin(runTask);

        Assert.Equal([1, 2], abandoned);
        Assert.Equal(0, consumer.Metrics.ConsumedItemCount);
        Assert.Equal(0, consumer.Metrics.FailedConsumptionCount);
        Assert.True(consumer.IsCompleted);
    }

    [Fact]
    public async Task Stop_Waits_For_Active_Batch_And_Abandons_Pending_Items()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var abandoned = new List<int>();
        await using var consumer = new RetryingBatchConsumer<int>(
            async (_, _) =>
            {
                entered.TrySetResult(true);
                await release.Task;
            },
            batchSize: 1,
            maxBatchInterval: TimeSpan.FromMinutes(1));
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
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 1)]
    public void Constructor_Rejects_Invalid_Configuration(int batchSize, int retryPartitionCount)
    {
        Assert.ThrowsAny<ArgumentException>(() => new RetryingBatchConsumer<int>(
            (_, _) => Task.CompletedTask,
            batchSize,
            TimeSpan.FromSeconds(1),
            retryPartitionCount: retryPartitionCount));
    }

    private static async Task CompletesWithin(Task task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(task, completed);
        await task;
    }
}
