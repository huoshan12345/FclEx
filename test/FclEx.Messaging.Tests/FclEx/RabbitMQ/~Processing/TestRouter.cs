using RabbitMQ.Client;

namespace FclEx.RabbitMQ;

public sealed class TestRouter : MessageRouter<string, string>
{
    protected override bool AutomaticRecoveryEnabled => false;

    private readonly Func<string, string> _keyFunc;

    private TestRouter(Func<string, string> keyFunc)
        : base(null, null, StringToStringConverter.Instance)
    {
        _keyFunc = Check.NotNull(keyFunc);
    }

    protected override string GetRoutingKey(IReadOnlyBasicProperties props, string output)
    {
        return _keyFunc(output);
    }

    protected override async ValueTask DisposeActionAsync()
    {
        if (Channel is not null && Settings is not null)
        {
            await StopConsumingAsync();

            await Channel.QueueDeleteAsync(Settings.RabbitMqQueue.Name, ifUnused: false, ifEmpty: false);
            await Channel.ExchangeDeleteAsync(Settings.Exchange.Name, ifUnused: false);

            if (Settings.TargetExchange.Name != Settings.Exchange.Name)
                await Channel.ExchangeDeleteAsync(Settings.TargetExchange.Name, ifUnused: false);
        }

        await base.DisposeActionAsync();
    }

    public static async Task<TestRouter> CreateAsync(RabbitMqRouterOptions settings, Func<string, string> keyFunc)
    {
        var publisher = new TestRouter(keyFunc);
        await publisher.InitializeAsync(settings);
        return publisher;
    }
}
