namespace FclEx.RabbitMQ;

public class MessageConverter<TSource, TDestination> : IMessageConverter<TSource, TDestination>
{
    protected readonly Func<TSource, Task<TDestination>> _handler;

    public MessageConverter(Func<TSource, Task<TDestination>> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Task<TDestination> Convert(TSource source)
    {
        return _handler(source);
    }
}