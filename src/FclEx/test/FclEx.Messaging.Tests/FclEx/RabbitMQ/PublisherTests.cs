namespace FclEx.RabbitMQ;

public class PublisherTests : RabbitMQTests
{
    public readonly ExchangeSettings DefaultExchange;

    public PublisherTests(RabbitMQFixture fixture) : base(fixture)
    {
        DefaultExchange = new()
        {
            Name = Fixture.WithAssemblyInfo("test.publisher"),
            Type = "topic",
            IsDelayed = true,
        }; ;
    }

    private Task<TestPublisher> CreatePublisher()
    {
        var connection = RmqConnection;
        return TestPublisher.CreateAsync(new PublisherSettings(connection, DefaultExchange));
    }

    [Fact]
    public async Task Publish_Test()
    {
        await using var publisher = await CreatePublisher();
        await publisher.PublishAsync("test", "test");
    }

    [Fact]
    public async Task Publish_Serially_Test()
    {
        await using var publisher = await CreatePublisher();
        for (var i = 0; i < 10; i++)
        {
            await publisher.PublishAsync("test", "test");
        }
    }

    [Fact]
    public async Task Publish_List_Test()
    {
        await using var publisher = await CreatePublisher();
        await publisher.PublishAsync<string>(Enumerable.Range(1, 10).Select(m => "test"), "test");
    }

    [Fact]
    public async Task Publish_Multi_Test()
    {
        await using var publisher1 = await CreatePublisher();
        await using var publisher2 = await CreatePublisher();
        await publisher1.PublishAsync("test", "test");
        await publisher2.PublishAsync("test", "test");
    }
}