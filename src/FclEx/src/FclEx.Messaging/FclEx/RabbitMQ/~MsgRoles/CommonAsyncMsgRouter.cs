using RabbitMQ.Client;

namespace FclEx.RabbitMQ;

public class CommonAsyncMsgRouter<TInput, TOutput> : AsyncMsgRouter<TInput, TOutput>
{
    protected readonly Func<TInput, Task<TOutput>> _handler;
    protected readonly Func<IBasicProperties, TOutput, string> _keyFunc;

    public CommonAsyncMsgRouter(Func<TInput, Task<TOutput>> handler,
        Func<IBasicProperties, TOutput, string> keyFunc,
        ILoggerFactory? logger = null,
        IMemoryBytesSerializer? serializer = null)
        : base(new CommonAsyncMsgConverter<TInput, TOutput>(handler), serializer, logger)
    {
        _keyFunc = keyFunc ?? throw new ArgumentNullException(nameof(keyFunc));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    protected override string GetRoutingKey(IBasicProperties props, TOutput output)
    {
        return _keyFunc(props, output);
    }
}