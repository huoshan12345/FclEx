using RabbitMQ.Client;

namespace FclEx.RabbitMQ;

public class TestPublisher<T> : MessagePublisher<T>
{
    protected override bool AutomaticRecoveryEnabled { get; } = false;

    public TestPublisher(PublisherSettings settings)
    {
        Init(settings);
    }

    protected override void DisposeInternal()
    {
        using var channel = Connection!.CreateChannel();
        channel.Model.ExchangeDelete(Settings!.Exchange.Name);
        base.DisposeInternal();
    }
}

public sealed class TestPublisher : TestPublisher<string>
{
    public TestPublisher(PublisherSettings settings) : base(settings)
    {
    }
}