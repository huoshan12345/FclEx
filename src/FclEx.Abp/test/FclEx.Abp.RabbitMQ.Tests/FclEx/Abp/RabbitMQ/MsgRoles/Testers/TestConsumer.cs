using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FclEx.Abp.RabbitMQ.MsgRoles.Testers;

public class TestConsumer<T> : CommonAsyncConsumer<T>
{
    public override int MaxRetryTimes { get; }
    protected override bool AutomaticRecoveryEnabled { get; } = false;
    protected readonly Func<int, TimeSpan>? _delay;

    public TestConsumer(ConsumerSettings settings, ConsumeHandler handler, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null) : base(handler)
    {
        _delay = delay;
        MaxRetryTimes = maxRetryTimes;
        Init(settings);
    }

    public TestConsumer(ConsumerSettings settings, Func<T, OperateResult> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : this(settings, (_, m) => Operate.Execute(() => action(m)), maxRetryTimes, delay)
    {
    }

    public TestConsumer(ConsumerSettings settings, Action<T> action, int maxRetryTimes = 3, Func<int, TimeSpan>? delay = null)
        : this(settings, m => Operate.Execute(() => action(m)), maxRetryTimes, delay)
    {
    }

    protected override void DisposeInternal()
    {
        Channel.QueueDelete(Settings!.Queue.Name);
        Channel.ExchangeDelete(Settings.Exchange.Name);
        base.DisposeInternal();
    }

    protected override Task OnConsumeErrorAsync(BasicDeliverEventArgs args, T input, Exception exception)
    {
        if (_delay != null)
        {
            var errorTimes = args.BasicProperties.GetErrorTimes();
            args.BasicProperties.SetDelay(_delay(errorTimes));
        }
        return base.OnConsumeErrorAsync(args, input, exception);
    }
}

public sealed class TestConsumer : TestConsumer<string>
{
    public TestConsumer(ConsumerSettings settings, ConsumeHandler handler, int maxRetryTimes = 3)
        : base(settings, handler, maxRetryTimes)
    {
    }

    public TestConsumer(ConsumerSettings settings, Action<string> action, int maxRetryTimes = 3)
        : base(settings, action, maxRetryTimes)
    {
    }

    public TestConsumer(ConsumerSettings settings, Func<string, OperateResult> action, int maxRetryTimes = 3)
        : base(settings, action, maxRetryTimes)
    {
    }
}