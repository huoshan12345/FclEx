using RabbitMQ.Client;

namespace FclEx.RabbitMQ;

public sealed class TestRouter : MessageRouter<string, string>
{
    protected override bool AutomaticRecoveryEnabled { get; } = false;

    private readonly Func<string, string> _routingKeySelector;

    public TestRouter(RouterSettings settings, Func<string, string> routingKeySelector)
        : base(StringToStringConverter.Instance)
    {
        _routingKeySelector = Check.NotNull(routingKeySelector);
        Init(settings);
    }

    protected override string GetRoutingKey(IBasicProperties props, string output)
    {
        return _routingKeySelector(output);
    }

    protected override void DisposeInternal()
    {
        Channel.QueueDelete(Settings!.Queue.Name);
        Channel.ExchangeDelete(Settings.Exchange.Name);
        Channel.ExchangeDelete(Settings.TargetExchange.Name);
        base.DisposeInternal();
    }
}