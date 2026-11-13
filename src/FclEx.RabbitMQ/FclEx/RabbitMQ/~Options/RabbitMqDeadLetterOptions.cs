namespace FclEx.RabbitMQ;

/// <summary>Names the durable direct exchange and quorum queue that retain failed messages.</summary>
public sealed class RabbitMqDeadLetterOptions
{
    /// <summary>Gets the exchange name, or null to use the source queue name followed by .dlx.</summary>
    public string? ExchangeName { get; init; }

    /// <summary>Gets the queue name, or null to use the source queue name followed by .dead.</summary>
    public string? QueueName { get; init; }

    /// <summary>Gets the dead-letter routing key, or null to use the source queue name.</summary>
    public string? RoutingKey { get; init; }
}
