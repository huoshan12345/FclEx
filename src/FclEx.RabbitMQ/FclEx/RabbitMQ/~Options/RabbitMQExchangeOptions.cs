namespace FclEx.RabbitMQ;

/// <summary>Describes an exchange to declare or an existing exchange to reference.</summary>
public sealed class RabbitMQExchangeOptions
{
    /// <summary>References the built-in default exchange without declaring it.</summary>
    public static RabbitMQExchangeOptions Default => new() { Declare = false, Type = ExchangeType.Direct };

    /// <summary>Gets the exchange name. Empty is allowed only when referencing the default exchange.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the standard exchange type: direct, topic, fanout, or headers.</summary>
    public string Type { get; init; } = ExchangeType.Topic;

    /// <summary>Gets whether to declare the exchange. Set false for predefined or externally managed exchanges.</summary>
    public bool Declare { get; init; } = true;

    /// <summary>Gets additional declaration arguments. Copied recursively when a role is created.</summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
}
