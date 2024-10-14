namespace FclEx.Consumers;

public class BatchRetryConsumerTests
{
    private record Model(int Number);

    private readonly ITestOutputHelper _output;

    public BatchRetryConsumerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Test()
    {
        const int retryTimes = 3;
        var numbers = Enumerable.Range(1, 10).Select(m => new Model(m)).ToArray();
        var consumer = new BatchRetryConsumer<Model>(5, TimeSpan.FromSeconds(1), retryTimes);
        consumer.ConsumingHandler += (sender, list) =>
        {
            _output.WriteLine(nameof(consumer.ConsumingHandler));
            if (list.Any(m => m.Number % 3 == 0))
                throw new Exception();
            return Task.CompletedTask;
        };
        consumer.ExceptionHandler += (sender, args) =>
        {
            _output.WriteLine(nameof(consumer.ExceptionHandler));
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (sender, args) =>
        {
            _output.WriteLine(nameof(consumer.DiscardHandler));
            Assert.NotNull(args.Exception);
            Assert.Equal(retryTimes, args.ErrorTimes);
        };
        consumer.AddRange(numbers);
        var task = consumer.Start();
        consumer.CompleteAdding();
        await task;

        var errors = numbers.Count(m => m.Number % 3 == 0);
        Assert.Equal(0, consumer.Count);
        Assert.Equal(numbers.Length - errors, consumer.Counter.Consume);
        Assert.Equal(errors * retryTimes, consumer.Counter.Exception);
        Assert.Equal(errors, consumer.Counter.Discard);
    }

    [Fact]
    public async Task Dispose_AfterStart_Test()
    {
        var consumer = new BatchRetryConsumer<Model>(5, TimeSpan.FromSeconds(1), 1);
        var task = consumer.Start();
        consumer.Dispose();
        await TaskHelper.Delay(1);
        Assert.True(task.IsCompleted);
    }

    [Fact]
    public async Task Dispose_DuringConsuming_Test()
    {
        var consumer = new BatchRetryConsumer<Model>(5, TimeSpan.FromSeconds(1), 1);
        consumer.ConsumingHandler += (sender, list) => TaskHelper.Delay(1);
        var task = consumer.Start();
        consumer.Add(new Model(0));
        consumer.Dispose();
        await TaskHelper.Delay(1);
        Assert.False(task.IsCompleted);
        var finishTask = await Task.WhenAny(task, TaskHelper.Delay(10));
        Assert.Equal(task, finishTask);
    }

    [Fact]
    public async Task CompleteAdding_BeforeStart_Test()
    {
        var consumer = new BatchRetryConsumer<int>(5, TimeSpan.FromSeconds(1), 1);
        consumer.ConsumingHandler += (sender, list) => Task.CompletedTask;
        consumer.AddRange(Enumerable.Range(1, 10));
        consumer.CompleteAdding();
        var r = await Operate.ExecuteAsync(() => consumer.Start(), TimeSpan.FromSeconds(5));
        Assert.True(r.Success);
        Assert.True(consumer.IsComplete);
        Assert.Equal(10, consumer.Counter.Consume);
    }

    [Fact]
    public async Task CompleteAdding_AfterStart_Test()
    {
        var consumer = new BatchRetryConsumer<int>(5, TimeSpan.FromSeconds(1), 1);
        consumer.ConsumingHandler += (sender, list) => Task.CompletedTask;
        var task = Operate.ExecuteAsync(() => consumer.Start(), TimeSpan.FromSeconds(5));
        consumer.AddRange(Enumerable.Range(1, 10));
        consumer.CompleteAdding();
        var r = await task;
        Assert.True(r.Success);
        Assert.True(consumer.IsComplete);
        Assert.Equal(10, consumer.Counter.Consume);
    }
}