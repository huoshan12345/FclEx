namespace FclEx.RabbitMQ;

public class PublisherTests(RabbitMqTestsFixture fixture) : RabbitMqTests(fixture)
{
    [Fact]
    public async Task Publish_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Publish_Test)),
        };
        await using var publisher = await TestPublisher.CreateAsync(new(RabbitMqTestsFixture.ConnectionSettings, exchange));
        await publisher.PublishAsync("test", "test");
    }

    [Fact]
    public async Task Publish_Serially_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Publish_Serially_Test)),
        };
        await using var publisher = await TestPublisher.CreateAsync(new(RabbitMqTestsFixture.ConnectionSettings, exchange));
        for (var i = 0; i < 10; i++)
        {
            await publisher.PublishAsync("test", "test");
        }
    }

    [Fact]
    public async Task Publish_List_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Publish_List_Test)),
        };
        await using var publisher = await TestPublisher.CreateAsync(new(RabbitMqTestsFixture.ConnectionSettings, exchange));
        await publisher.PublishAsync<string>(Enumerable.Range(1, 10).Select(m => "test"), "test");
    }

    [Fact]
    public async Task Publish_Multi_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Publish_Multi_Test)),
        };
        await using var publisher1 = await TestPublisher.CreateAsync(new(RabbitMqTestsFixture.ConnectionSettings, exchange));
        await using var publisher2 = await TestPublisher.CreateAsync(new(RabbitMqTestsFixture.ConnectionSettings, exchange));
        await publisher1.PublishAsync("test", "test");
        await publisher2.PublishAsync("test", "test");
    }
}