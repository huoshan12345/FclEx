using RabbitMQ.Client;

namespace FclEx.RabbitMQ;

public class TestPublisher : MessagePublisher
{
    private TestPublisher() : base(null, null) { }

    protected override bool AutomaticRecoveryEnabled => false;

    protected override async ValueTask DisposeActionAsync()
    {
        if (Connection is null || Settings is null)
            return;

        await using var channel = await Connection.CreateAutoCloseableChannelAsync();
        await channel.Value.ExchangeDeleteAsync(Settings.Exchange.Name);
        await base.DisposeActionAsync();
    }

    public static async Task<TestPublisher> CreateAsync(RabbitMqPublisherOptions settings)
    {
        var publisher = new TestPublisher();
        await publisher.InitializeAsync(settings);
        return publisher;
    }
}