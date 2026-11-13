namespace FclEx.RabbitMQ;

/// <summary>Configures broker-native quorum queue retries; this is not an application retry loop.</summary>
public sealed class RabbitMqRetryOptions
{
    /// <summary>Gets whether handler failures are returned to the queue. If false, they are dead-lettered.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Gets the positive base delay. The broker applies linear backoff using its delivery count.</summary>
    public TimeSpan MinDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Gets the positive delay cap, which must be at least <see cref="MinDelay"/>.</summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Gets the broker's positive failed-delivery limit, including failures caused by connection loss.</summary>
    /// <remarks>This is not an exact maximum number of business-handler invocations.</remarks>
    public int DeliveryLimit { get; init; } = 20;
}
