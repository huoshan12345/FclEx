namespace FclEx.RabbitMQ;

/// <summary>Configures a subscription whose topology is prepared before consumption starts.</summary>
public sealed class RabbitMQConsumerOptions
{
    /// <summary>Gets the input quorum queue and its shared failure policy.</summary>
    public RabbitMQQueueOptions Queue { get; init; } = new();

    /// <summary>Gets the bindings to ensure. Empty permits publishing through the default exchange by queue name.</summary>
    public IReadOnlyList<RabbitMQBindingOptions> Bindings { get; init; } = [];

    /// <summary>Gets the positive maximum number of unacknowledged deliveries; must be at least MaxConcurrency.</summary>
    public ushort PrefetchCount { get; init; } = 1;

    /// <summary>Gets the positive handler concurrency for this subscription. Concurrent completion order is unspecified.</summary>
    public ushort MaxConcurrency { get; init; } = 1;

    /// <summary>Gets the positive time allowed to cancel and drain callbacks before closing their channel.</summary>
    /// <remarks>User callbacks must cooperate with cancellation; closing a channel cannot terminate application code.</remarks>
    public TimeSpan ShutdownTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
