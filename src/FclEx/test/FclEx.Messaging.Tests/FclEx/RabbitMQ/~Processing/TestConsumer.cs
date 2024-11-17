using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FclEx.RabbitMQ;

public class TestConsumer<T> : CommonConsumer<T>
{
    protected override int MaxRetryTimes { get; }
    protected override bool AutomaticRecoveryEnabled => false;
    protected readonly Func<int, TimeSpan>? _delay;

    protected TestConsumer(ConsumeHandler handler, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null) : base(handler)
    {
        _delay = delay;
        MaxRetryTimes = maxRetryTimes;
    }

    protected TestConsumer(Func<T, OperateResult> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : this((_, m) => Operate.Execute(() => action(m)), maxRetryTimes, delay)
    {
    }

    protected TestConsumer(Action<T> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : this(m => Operate.Execute(() => action(m)), maxRetryTimes, delay)
    {
    }

    protected override async ValueTask DisposeActionAsync()
    {
        if (Settings is null || Channel is null)
            return;

        await Channel.QueueDeleteAsync(Settings.Queue.Name);
        await Channel.ExchangeDeleteAsync(Settings.Exchange.Name);
        await base.DisposeActionAsync();
    }

    protected override Task OnConsumeErrorAsync(BasicDeliverEventArgs args, T input, Exception exception)
    {
        var properties = args.BasicProperties.AsBasicProperties();
        if (_delay != null)
        {
            var errorTimes = properties.GetErrorTimes();
            properties.SetDelay(_delay(errorTimes));
        }
        return base.OnConsumeErrorAsync(args, input, exception);
    }

    public static async Task<TestConsumer<T>> CreateAsync(ConsumerSettings settings, Action<T> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
    {
        var publisher = new TestConsumer<T>(action, maxRetryTimes, delay);
        await publisher.InitializeAsync(settings);
        return publisher;
    }
}

public sealed class TestConsumer : TestConsumer<string>
{
    private TestConsumer(ConsumeHandler handler, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : base(handler, maxRetryTimes, delay)
    {
    }

    private TestConsumer(Func<string, OperateResult> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : base(action, maxRetryTimes, delay)
    {
    }

    private TestConsumer(Action<string> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : base(action, maxRetryTimes, delay)
    {
    }
}