namespace FclEx.RabbitMQ;

public interface IMessagePublisher : IMessageProcessor<RabbitMqPublisherOptions>
{
    Task PublishAsync<T>(IEnumerable<RoutingMessage<T>> messages);
}

public static class MessagePublisherExtensions
{
    public static Task PublishAsync<T>(this IMessagePublisher publisher, RoutingMessage<T> message)
    {
        return publisher.PublishAsync([message]);
    }

    public static Task PublishAsync<T>(this IMessagePublisher publisher, T message, string routingKey, TimeSpan delay = default, string messageId = "")
    {
        return publisher.PublishAsync<T>((message, routingKey, delay, messageId));
    }

    public static Task PublishAsync<T>(this IMessagePublisher publisher, IEnumerable<T> messages, string routingKey, TimeSpan delay = default)
    {
        return publisher.PublishAsync(messages.Select(m => (RoutingMessage<T>)(m, routingKey, delay, "")));
    }

    public static Task PublishAsync<TSource, T>(this IMessagePublisher publisher, IEnumerable<TSource> source, Func<TSource, (T body, string routingKey, TimeSpan delay, string id)> selector)
    {
        return publisher.PublishAsync(source.Select(m => (RoutingMessage<T>)selector(m)));
    }

    public static Task PublishAsync<TSource, T>(this IMessagePublisher publisher, IEnumerable<TSource> source, Func<TSource, (T body, string routingKey)> selector)
    {
        return publisher.PublishAsync(source.Select(m => (RoutingMessage<T>)selector(m)));
    }
}