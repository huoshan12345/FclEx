namespace FclEx.RabbitMQ;

public class CommonPublisher<TMsg> : Publisher<TMsg>
{
    public CommonPublisher(ILoggerFactory? logger = null, IMemoryBytesSerializer? serializer = null)
        : base(serializer, logger)
    {
    }
}