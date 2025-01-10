namespace FclEx.RabbitMQ;

public class CommonConsumer<T> : MessageConsumer<T>
{
    protected ConsumeHandler Handler { get; }

    public CommonConsumer(ConsumeHandler handler, ILoggerFactory? loggerFactory = null, IMemoryBytesSerializer? serializer = null) : base(loggerFactory, serializer)
    {
        Handler = Check.NotNull(handler);
    }

    protected override Task<OperationResult> ConsumeActionAsync(BasicDeliverEventArgs args, T message)
    {
        return Handler(args, message);
    }
}