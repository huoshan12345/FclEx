using System.Diagnostics;

namespace FclEx.RabbitMQ;

public class RabbitMqConsumerIntegrationTests
{
    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task ParallelHandlers_RespectConfiguredConcurrencyAndPrefetch()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var prepared = scope.Input("parallel");
        var input = new RabbitMqConsumerOptions { Queue = prepared.Queue, MaxConcurrency = 2, PrefetchCount = 2 };
        var active = 0;
        var completed = 0;
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bothStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var allCompleted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, async (_, _) =>
        {
            var count = Interlocked.Increment(ref active);
            Assert.InRange(count, 1, 2);
            if (count == 2) bothStarted.TrySetResult(true);
            await release.Task;
            Interlocked.Decrement(ref active);
            if (Interlocked.Increment(ref completed) == 4) allCompleted.TrySetResult(true);
            return MessageHandlingResult.Completed;
        });
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = consumer.RunAsync(stop.Token);
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMqExchangeOptions.Default });
        try
        {
            for (var i = 0; i < 4; i++) await publisher.PublishAsync(new(i.ToString(), new() { RoutingKey = input.Queue.Name }));
            await bothStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(2, Volatile.Read(ref active));
            release.TrySetResult(true);
            await allCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally { release.TrySetResult(true); stop.Cancel(); await run; }
        Assert.Equal(4, completed);
    }

    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task NativeRetry_StaysInOriginalQueue_AndBacksOff()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var exchange = scope.Exchange("events");
        var input = scope.Input("retrying", exchange);
        var observer = scope.Input("other-subscriber", exchange);
        await using var idle = await scope.Client.CreateConsumerAsync<string>(observer, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        var deliveries = new List<ReceivedMessage<string>>();
        var watch = Stopwatch.StartNew();
        TimeSpan first = default;
        TimeSpan second = default;
        var handled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, (message, _) =>
        {
            deliveries.Add(message);
            if (deliveries.Count == 1)
            {
                first = watch.Elapsed;
                return Task.FromResult(MessageHandlingResult.Retry);
            }
            second = watch.Elapsed;
            handled.TrySetResult(true);
            return Task.FromResult(MessageHandlingResult.Completed);
        });
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = consumer.RunAsync(stop.Token);
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = exchange });
        await publisher.PublishAsync(new("message", messageId: "same-id"));
        try { await handled.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { stop.Cancel(); await run; }
        Assert.Equal(2, deliveries.Count);
        Assert.True(deliveries[1].DeliveryCount > deliveries[0].DeliveryCount);
        Assert.Equal("same-id", deliveries[1].MessageId);
        Assert.True(second - first >= TimeSpan.FromMilliseconds(90));
        Assert.Equal("message", BrokerScope.Text(await scope.GetAsync(observer.Queue.Name)));
        await scope.AssertEmptyForAsync(observer.Queue.Name, TimeSpan.FromMilliseconds(250));
    }

    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task DeliveryLimit_RetainsPoisonMessageInDeadLetterQueue()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var input = scope.Input("poison", retry: new() { MinDelay = TimeSpan.FromMilliseconds(50), MaxDelay = TimeSpan.FromMilliseconds(50), DeliveryLimit = 2 });
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, (_, _) => throw new InvalidOperationException("poison"));
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = consumer.RunAsync(stop.Token);
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMqExchangeOptions.Default });
        await publisher.PublishAsync(new("poison-body", new() { RoutingKey = input.Queue.Name }));
        try
        {
            var dead = await scope.GetAsync(input.Queue.Name + ".dead");
            Assert.Equal("poison-body", BrokerScope.Text(dead));
            Assert.Equal("delivery_limit", System.Text.Encoding.UTF8.GetString(Assert.IsType<byte[]>(dead.BasicProperties.Headers!["x-first-death-reason"])));
        }
        finally { stop.Cancel(); await run; }
        await scope.AssertEmptyForAsync(input.Queue.Name, TimeSpan.FromMilliseconds(100));
    }

    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task CanceledWork_IsRequeuedWithoutDeletingTopology()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var input = scope.Input("canceled");
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, async (_, token) =>
        {
            entered.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, token);
            return MessageHandlingResult.Completed;
        });
        var run = consumer.RunAsync(stop.Token);
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMqExchangeOptions.Default });
        await publisher.PublishAsync(new("unfinished", new() { RoutingKey = input.Queue.Name }));
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        stop.Cancel();
        await run;
        Assert.Equal("unfinished", BrokerScope.Text(await scope.GetAsync(input.Queue.Name)));
    }
}
