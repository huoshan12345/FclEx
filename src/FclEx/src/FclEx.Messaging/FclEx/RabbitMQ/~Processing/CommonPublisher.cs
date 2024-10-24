using FclEx.Serialization;

namespace FclEx.RabbitMQ;

public class CommonPublisher<T> : MessagePublisher<T>
{
    public CommonPublisher(ILoggerFactory? logger = null, IMemoryBytesSerializer? serializer = null)
        : base(serializer, logger)
    {
    }
}