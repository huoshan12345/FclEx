using FclEx.Serialization;

namespace FclEx.RabbitMQ;

public class CommonRouter<TInput, TOutput> : MessageRouter<TInput, TOutput>
{
    protected readonly Func<TInput, Task<TOutput>> _handler;
    protected readonly Func<IBasicProperties, TOutput, string> _keyFunc;

    public CommonRouter(Func<TInput, Task<TOutput>> handler,
        Func<IBasicProperties, TOutput, string> keyFunc,
        ILoggerFactory? logger = null,
        IMemoryBytesSerializer? serializer = null)
        : base(new MessageConverter<TInput, TOutput>(handler), serializer, logger)
    {
        _keyFunc = keyFunc ?? throw new ArgumentNullException(nameof(keyFunc));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    protected override string GetRoutingKey(IBasicProperties props, TOutput output)
    {
        return _keyFunc(props, output);
    }
}