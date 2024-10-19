namespace FclEx.RabbitMQ;

public interface IMessageConverter<in TSource, TDestination>
{
    Task<TDestination> ConvertAsync(TSource source);
}