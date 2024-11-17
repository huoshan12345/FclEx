namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public class CommonConsumer<T> : MessageConsumer<T>
{
    protected ConsumeHandler Handler { get; }

    public CommonConsumer(ConsumeHandler handler, ILoggerFactory? loggerFactory = null, IMemoryBytesSerializer? serializer = null) : base(loggerFactory, serializer)
    {
        Handler = Check.NotNull(handler);
    }

    protected override Task<OperateResult> ConsumeActionAsync(BasicDeliverEventArgs args, T message)
    {
        return Handler(args, message);
    }
}