using Meziantou.Xunit.v3;

namespace FclEx.Utils.Consumers;

public class BatchRetryConsumerTests
{
    private record Model(int Number);

    [Fact]
    public async Task Test()
    {
        const int retryTimes = 2;
        const int batchSize = 5;
        const int total = 10;
        const int retryPartCount = 3;
        var numbers = Enumerable.Range(1, total).Select(m => new Model(m)).ToArray();
        var consumer = new BatchRetryConsumer<Model>(batchSize, TimeSpan.FromMilliseconds(100), retryTimes, retryPartCount);
        consumer.ConsumingHandler += (sender, list) =>
        {
            if (list.Any(m => m.Number % 3 == 0))
                throw new Exception();
            return Task.CompletedTask;
        };
        consumer.ExceptionHandler += (sender, args) =>
        {
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (sender, args) =>
        {
            Assert.NotNull(args.Exception);
            Assert.Equal(retryTimes, args.ErrorTimes);
        };
        consumer.ExceptionLogger += (sender, ex, message) => { };
        consumer.AddRange(numbers);
        var task = consumer.StartAsync();
        consumer.CompleteAdding();
        await task;

        var errors = numbers.Count(m => m.Number % 3 == 0);
        Assert.Equal(0, consumer.Count);
        Assert.Equal(numbers.Length - errors, consumer.Counter.Consume);
        Assert.Equal(errors, consumer.Counter.Discard);
    }

    [DisableParallelization]
    [RetryFact(3, 100)]
    public async Task Dispose_AfterStart_Test()
    {
        var consumer = new BatchRetryConsumer<Model>(5, TimeSpan.FromMilliseconds(100), 1);
        var task = consumer.StartAsync();
        consumer.Dispose();
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task Dispose_Waits_For_Active_Consumption()
    {
        var consumer = new BatchRetryConsumer<Model>(5, TimeSpan.FromMilliseconds(100), 1);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        consumer.ConsumingHandler += async (_, _) =>
        {
            entered.TrySetResult(true);
            await release.Task;
        };

        var task = consumer.StartAsync();
        consumer.Add(new Model(0));
        Assert.Same(entered.Task, await Task.WhenAny(entered.Task, Task.Delay(TimeSpan.FromSeconds(10))));

        var disposeTask = Task.Run(consumer.Dispose);
        await Task.Delay(TimeSpan.FromMilliseconds(50));
        Assert.False(disposeTask.IsCompleted);

        release.SetResult(true);
        await disposeTask;
        Assert.True(task.IsCompleted);
    }

    [RetryFact]
    public async Task CompleteAdding_BeforeStart_Test()
    {
        var consumer = new BatchRetryConsumer<int>(5, TimeSpan.FromMilliseconds(100), 1);
        consumer.ConsumingHandler += (sender, list) => Task.CompletedTask;
        consumer.AddRange(Enumerable.Range(1, 10));
        consumer.CompleteAdding();
        var r = await Operation.ExecuteAsync(() => consumer.StartAsync(), TimeSpan.FromSeconds(5));
        Assert.True(r.IsSuccess);
        Assert.True(consumer.IsComplete);
        Assert.Equal(10, consumer.Counter.Consume);
    }

    [RetryFact]
    public async Task CompleteAdding_AfterStart_Test()
    {
        var consumer = new BatchRetryConsumer<int>(5, TimeSpan.FromMilliseconds(100), 1);
        consumer.ConsumingHandler += (sender, list) => Task.CompletedTask;
        var task = Operation.ExecuteAsync(() => consumer.StartAsync(), TimeSpan.FromSeconds(5));
        consumer.AddRange(Enumerable.Range(1, 10));
        consumer.CompleteAdding();
        var r = await task;
        Assert.True(r.IsSuccess);
        Assert.True(consumer.IsComplete);
        Assert.Equal(10, consumer.Counter.Consume);
    }
}
