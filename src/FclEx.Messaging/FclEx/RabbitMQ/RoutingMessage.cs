namespace FclEx.RabbitMQ;

public readonly struct RoutingMessage<T>
{
    public readonly T Body;
    public readonly string RoutingKey;
    public readonly string? Id;
    public readonly TimeSpan Delay;

    public RoutingMessage(T body, string routingKey, TimeSpan delay = default, string id = "")
    {
        Body = body;
        RoutingKey = routingKey;
        Id = id;
        Delay = delay;
    }

    public static implicit operator RoutingMessage<T>((T body, string routingKey, TimeSpan delay, string id) tuple)
    {
        return new(tuple.body, tuple.routingKey, tuple.delay, tuple.id);
    }

    public static implicit operator RoutingMessage<T>((T body, string routingKey) tuple)
    {
        return new(tuple.body, tuple.routingKey);
    }
}