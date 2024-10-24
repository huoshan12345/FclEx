using FclEx.Serialization;

namespace FclEx.RabbitMQ;

public class CommonConsumer<TMessage> : MessageConsumer<TMessage>
{
    protected readonly ConsumeHandler _handler;

    public CommonConsumer(ConsumeHandler handler, IMemoryBytesSerializer? serializer = null,
        ILoggerFactory? logger = null)
        : base(serializer, logger)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    protected override Task<OperateResult> ConsumeInternalAsync(BasicDeliverEventArgs args, TMessage message)
    {
        return _handler(args, message);
    }
}