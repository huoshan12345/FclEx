namespace FclEx.RabbitMQ;

/// <summary>Configures a one-input, one-output conversion router with independent consumption and publishing channels.</summary>
public sealed class RabbitMqRouterOptions
{
    /// <summary>Gets the input subscription and failure policy.</summary>
    public RabbitMqConsumerOptions Input { get; init; } = new();

    /// <summary>Gets the target publisher configuration, including the exchange to declare before consumption.</summary>
    public RabbitMqPublisherOptions Target { get; init; } = new();

    /// <summary>Gets default output routing and scheduling options, used when no output selector is supplied.</summary>
    public RabbitMqPublishOptions Output { get; init; } = new();

    /// <summary>Gets the explicit allowlist of business header names to copy to output messages.</summary>
    /// <remarks>Broker scheduling, death, delivery-count and confirmation headers are forbidden.</remarks>
    public IReadOnlyList<string> ForwardedHeaderNames { get; init; } = [];
}
