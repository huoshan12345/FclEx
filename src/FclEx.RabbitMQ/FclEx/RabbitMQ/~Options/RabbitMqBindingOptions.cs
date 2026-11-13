namespace FclEx.RabbitMQ;

/// <summary>Describes a queue binding and the exchange that must exist before binding.</summary>
public sealed class RabbitMqBindingOptions
{
    /// <summary>Gets the source exchange. Explicit bindings to the default exchange are not supported.</summary>
    public RabbitMqExchangeOptions Exchange { get; init; } = new();

    /// <summary>Gets the routing key or topic pattern; it is ignored by fanout and headers exchanges.</summary>
    public string RoutingKey { get; init; } = "";

    /// <summary>Gets additional binding arguments, such as x-match and header matching values.</summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
}
