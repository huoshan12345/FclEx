namespace FclEx.RabbitMQ;

/// <summary>Configures a publisher that waits for broker confirmations and rejects unroutable messages.</summary>
public sealed class RabbitMqPublisherOptions
{
    /// <summary>Gets the fixed destination exchange to ensure before publishing.</summary>
    public RabbitMqExchangeOptions Exchange { get; init; } = new();

    /// <summary>Gets the positive per-publication timeout, including waiting for the publisher's channel.</summary>
    /// <remarks>An expired timeout does not prove that the broker rejected or never received the message.</remarks>
    public TimeSpan ConfirmationTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
