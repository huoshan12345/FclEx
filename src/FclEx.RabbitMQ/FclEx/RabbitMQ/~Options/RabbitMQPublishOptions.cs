namespace FclEx.RabbitMQ;

/// <summary>Configures routing and optional initial delivery scheduling for an individual message.</summary>
public sealed class RabbitMQPublishOptions
{
    /// <summary>Gets the routing key. Empty is valid; the UTF-8 representation must fit an AMQP short string.</summary>
    public string RoutingKey { get; init; } = "";

    /// <summary>Gets a nonnegative delay relative to the start of each PublishAsync call. Requires RabbitMQ 4.4 for a positive delay.</summary>
    public TimeSpan? Delay { get; init; }

    /// <summary>Gets an absolute earliest delivery time. Mutually exclusive with Delay; past times publish immediately.</summary>
    /// <remarks>Use a stable absolute time when retries must preserve a schedule.</remarks>
    public DateTimeOffset? DeliverAt { get; init; }
}
