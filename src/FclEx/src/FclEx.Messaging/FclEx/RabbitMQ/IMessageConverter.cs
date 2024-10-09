namespace FclEx.RabbitMQ;

public interface IMessageConverter<in TSource, TDestination>
{
    Task<TDestination> Convert(TSource source);
}