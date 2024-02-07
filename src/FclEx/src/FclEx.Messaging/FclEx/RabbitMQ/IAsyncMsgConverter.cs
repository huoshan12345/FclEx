namespace FclEx.RabbitMQ;

public interface IAsyncMsgConverter<in TSource, TDestination>
{
    Task<TDestination> Convert(TSource source);
}