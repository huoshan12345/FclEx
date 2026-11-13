using System.Text;

namespace FclEx.RabbitMQ;

public class RabbitMqRouterIntegrationTests
{
    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task Router_DeclaresTarget_ConvertsAndForwardsOnlyBusinessHeaders()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var source = scope.Input("source");
        var target = scope.Exchange("target");
        await using var router = await scope.Client.CreateRouterAsync(new RabbitMqRouterOptions
        {
            Input = source,
            Target = new() { Exchange = target },
            Output = new() { RoutingKey = "converted" },
            ForwardedHeaderNames = ["tenant"],
        }, new MessageConverter<int, string>((message, _) => Task.FromResult("number:" + message.Body)));
        // Router creation must already have declared the target before its subscription is started.
        await scope.Raw.ExchangeDeclarePassiveAsync(target.Name);
        var destination = scope.Input("destination", target);
        await using var idle = await scope.Client.CreateConsumerAsync<string>(destination, (_, _) => Task.FromResult(MessageHandlingResult.Completed));
        await scope.Raw.BasicPublishAsync("", source.Queue.Name, mandatory: true, basicProperties: new BasicProperties
        {
            MessageId = "input-stable-id",
            CorrelationId = "workflow-id",
            ContentType = "wrong-input-type",
            Headers = new Dictionary<string, object?> { ["tenant"] = "tenant-a", ["d-error-times"] = 7, ["x-opt-delivery-time"] = 1L },
        }, body: Encoding.UTF8.GetBytes("42"));
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = router.RunAsync(stop.Token);
        try
        {
            var output = await scope.GetAsync(destination.Queue.Name);
            Assert.Equal("number:42", BrokerScope.Text(output));
            Assert.Equal("converted", output.RoutingKey);
            Assert.Equal("input-stable-id", output.BasicProperties.MessageId);
            Assert.Equal("workflow-id", output.BasicProperties.CorrelationId);
            Assert.Equal("text/plain", output.BasicProperties.ContentType);
            var headers = output.BasicProperties.Headers!;
            Assert.Equal("tenant-a", Encoding.UTF8.GetString(Assert.IsType<byte[]>(headers["tenant"])));
            Assert.False(headers.ContainsKey("d-error-times"));
            Assert.False(headers.ContainsKey("x-opt-delivery-time"));
            Assert.False(headers.ContainsKey("x-death"));
        }
        finally { stop.Cancel(); await run; }
        await scope.AssertEmptyForAsync(source.Queue.Name, TimeSpan.FromMilliseconds(100));
    }

    [Fact(Skip = RabbitMqIntegrationFixture.SkipReason, SkipUnless = nameof(RabbitMqIntegrationFixture.IsConfigured), SkipType = typeof(RabbitMqIntegrationFixture))]
    public async Task UnroutableOutput_DoesNotAcknowledgeInput_AndEventuallyDeadLetters()
    {
        await using var scope = await BrokerScope.CreateAsync();
        var input = scope.Input("source", retry: new() { MinDelay = TimeSpan.FromMilliseconds(50), MaxDelay = TimeSpan.FromMilliseconds(50), DeliveryLimit = 1 });
        await using var router = await scope.Client.CreateRouterAsync(new RabbitMqRouterOptions
        {
            Input = input,
            Target = new() { Exchange = scope.Exchange("empty-target") },
        }, new MessageConverter<string, string>((message, _) => Task.FromResult(message.Body)));
        using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var run = router.RunAsync(stop.Token);
        await using var publisher = await scope.Client.CreatePublisherAsync<string>(new() { Exchange = RabbitMqExchangeOptions.Default });
        await publisher.PublishAsync(new("retained", new() { RoutingKey = input.Queue.Name }));
        try { Assert.Equal("retained", BrokerScope.Text(await scope.GetAsync(input.Queue.Name + ".dead"))); }
        finally { stop.Cancel(); await run; }
    }
}
