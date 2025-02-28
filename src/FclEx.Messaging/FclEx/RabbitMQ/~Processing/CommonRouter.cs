namespace FclEx.RabbitMQ;

public class CommonRouter<TInput, TOutput> : MessageRouter<TInput, TOutput>
{
    protected Func<TInput, Task<TOutput>> Handler { get; }
    protected Func<IReadOnlyBasicProperties, TOutput, string> KeyFunc { get; }

    public CommonRouter(Func<TInput, Task<TOutput>> handler,
        Func<IReadOnlyBasicProperties, TOutput, string> keyFunc,
        ILoggerFactory? logger = null,
        IMemoryBytesSerializer? serializer = null)
        : base(logger, serializer, new MessageConverter<TInput, TOutput>(handler))
    {
        KeyFunc = Check.NotNull(keyFunc);
        Handler = Check.NotNull(handler);
    }

    protected override string GetRoutingKey(IReadOnlyBasicProperties properties, TOutput output)
    {
        return KeyFunc(properties, output);
    }
}