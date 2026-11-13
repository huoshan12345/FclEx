using System.Diagnostics;
using RabbitMQ.Client.Exceptions;

namespace FclEx.RabbitMQ;

public class RabbitMQPublisherIntegrationTests
{
    [Fact(Skip = RabbitMQIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMQIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMQIntegrationFixture))]
    public async Task NoQueue_ReturnsFailure_AndQueueCanBePreparedBeforeRun()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var exchange = scope.Exchange("events");
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = exchange });
        await Assert.ThrowsAsync<PublishReturnException>(() => publisher.PublishAsync(new("unroutable")));
        var input = scope.Input("prepared", exchange);
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        await publisher.PublishAsync(new("retained-before-consumption"));
        Assert.Equal("retained-before-consumption", BrokerScope.Text(await scope.GetAsync(input.Queue.Name)));
    }

    [Fact(Skip = RabbitMQIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMQIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMQIntegrationFixture))]
    public async Task NativeDelay_ConfirmsBeforeDue_AndDoesNotBlockImmediateMessages()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var exchange = scope.Exchange("events");
        var input = scope.Input("input", exchange);
        await using var consumer = await scope.Client.CreateConsumerAsync<string>(input, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = exchange });
        var watch = Stopwatch.StartNew();
        await publisher.PublishAsync(new("delayed", new() { Delay = TimeSpan.FromSeconds(3) })).WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(3));
        await publisher.PublishAsync(new("immediate"));
        Assert.Equal("immediate", BrokerScope.Text(await scope.GetAsync(input.Queue.Name)));
        await scope.AssertEmptyForAsync(input.Queue.Name, TimeSpan.FromMilliseconds(250));
        var delayed = await scope.GetAsync(input.Queue.Name);
        Assert.Equal("delayed", BrokerScope.Text(delayed));
        Assert.True(watch.Elapsed >= TimeSpan.FromSeconds(3), "The scheduled message arrived before its delay elapsed.");
    }

    [Fact(Skip = RabbitMQIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMQIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMQIntegrationFixture))]
    public async Task NativeDelay_RoutesAtPublication_NotWhenDue()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var exchange = scope.Exchange("events");
        var first = scope.Input("first", exchange);
        await using var consumer1 = await scope.Client.CreateConsumerAsync<string>(first, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = exchange });
        await publisher.PublishAsync(new("scheduled", new() { DeliverAt = DateTimeOffset.UtcNow.AddSeconds(1) }));
        var late = scope.Input("late", exchange);
        await using var consumer2 = await scope.Client.CreateConsumerAsync<string>(late, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        Assert.Equal("scheduled", BrokerScope.Text(await scope.GetAsync(first.Queue.Name)));
        await scope.AssertEmptyForAsync(late.Queue.Name, TimeSpan.FromMilliseconds(250));
    }

    [Fact(Skip = RabbitMQIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMQIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMQIntegrationFixture))]
    public async Task DefaultExchange_AddressesQueueByName()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var input = scope.Input("direct");
        await using var consumer = await scope.Client.CreateConsumerAsync<int>(input, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        await using var publisher = await scope.Client.CreatePublisherAsync<int>(new() { Exchange = RabbitMQExchangeOptions.Default });
        await publisher.PublishAsync(new(42, new() { RoutingKey = input.Queue.Name }));
        Assert.Equal("42", BrokerScope.Text(await scope.GetAsync(input.Queue.Name)));
    }
}
