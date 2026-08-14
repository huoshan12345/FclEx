using System.Collections.Concurrent;

namespace FclEx.Utils.Consumers;

public class AutoRetryConsumerTests
{
    [Fact]
    public async Task Test()
    {
        using var consumer = new AutoRetryConsumer<int>(2);
        consumer.ConsumingHandler += (sender, _) => throw new Exception();
        var exceptions = 0;
        consumer.ExceptionHandler += (sender, args) =>
        {
            exceptions++;
            Assert.NotNull(args.Exception);
        };
        consumer.DiscardHandler += (_, args) =>
        {
            Assert.NotNull(args.Exception);
        };

        var task = consumer.StartAsync();
        // ReSharper disable once AccessToDisposedClosure
        var items = Enumerable.Range(1, 3)
            .Do(m => consumer.Add(m))
            .ToArray();
        consumer.CompleteAdding();
        await task;

        Assert.Equal(0, consumer.Count);
        Assert.Equal(0, consumer.Counter.Consume);
        Assert.Equal(exceptions, consumer.Counter.Exception);
        Assert.Equal(items.Length, consumer.Counter.Discard);
    }

    [Fact]
    public async Task Add_And_CompleteAdding_Are_Atomic()
    {
        using var consumer = new AutoRetryConsumer<int>(takeTimeout: TimeSpan.FromMilliseconds(100));
        var consumed = new ConcurrentBag<int>();
        consumer.ConsumingHandler += (_, item) =>
        {
            consumed.Add(item);
            return Task.CompletedTask;
        };

        var runTask = consumer.StartAsync();
        var accepted = new ConcurrentBag<int>();
        using var start = new ManualResetEventSlim();
        var producers = Enumerable.Range(0, 4).Select(producer => Task.Run(() =>
        {
            start.Wait();
            for (var i = 0; i < 250; i++)
            {
                var item = producer * 250 + i;
                try
                {
                    consumer.Add(item);
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
        await runTask;

        Assert.Equal(accepted.OrderBy(x => x), consumed.OrderBy(x => x));
    }
}
