namespace FclEx.RabbitMQ;

/// <summary>A deserialized delivery with copied metadata and no manual settlement handle.</summary>
/// <typeparam name="T">The deserialized value type.</typeparam>
public sealed class ReceivedMessage<T>
{
    private readonly Dictionary<string, object?> _headers;

    internal ReceivedMessage(T body, BasicDeliverEventArgs args)
    {
        Body = body;
        Exchange = args.Exchange;
        RoutingKey = args.RoutingKey;
        Redelivered = args.Redelivered;
        MessageId = args.BasicProperties.MessageId;
        CorrelationId = args.BasicProperties.CorrelationId;
        _headers = HeaderSnapshot.Copy(args.BasicProperties.Headers);
        DeliveryCount = Counter("x-delivery-count");
        AcquiredCount = Counter("x-acquired-count");
    }

    /// <summary>Gets the deserialized value; null is permitted when supported by its serializer.</summary>
    public T Body { get; }
    /// <summary>Gets the original message ID, which can be absent for external producers.</summary>
    public string? MessageId { get; }
    /// <summary>Gets the original optional correlation identity.</summary>
    public string? CorrelationId { get; }
    /// <summary>Gets the exchange reported on this delivery.</summary>
    public string Exchange { get; }
    /// <summary>Gets the routing key reported on this delivery.</summary>
    public string RoutingKey { get; }
    /// <summary>Gets whether the broker marked this delivery as a redelivery; it does not prove prior business execution.</summary>
    public bool Redelivered { get; }
    /// <summary>Gets the broker-reported failed-delivery count, or zero when absent.</summary>
    public long DeliveryCount { get; }
    /// <summary>Gets the broker-reported acquisition count, or zero when absent.</summary>
    public long AcquiredCount { get; }
    /// <summary>Gets an independent, read-only snapshot, including broker metadata for inspection.</summary>
    public IReadOnlyDictionary<string, object?> Headers => HeaderSnapshot.ReadOnly(_headers);

    private long Counter(string key)
        => _headers.TryGetValue(key, out var value) ? value switch { long n => n, int n => n, short n => n, byte n => n, _ => 0 } : 0;

    internal Dictionary<string, object?> ForwardHeaders(IEnumerable<string> names)
    {
        var selected = new Dictionary<string, object?>();
        foreach (var name in names)
            if (_headers.TryGetValue(name, out var value))
                selected[name] = value;
        return HeaderSnapshot.Copy(selected, rejectReserved: true);
    }
}
