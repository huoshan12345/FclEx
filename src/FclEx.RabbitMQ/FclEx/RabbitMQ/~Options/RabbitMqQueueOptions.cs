namespace FclEx.RabbitMQ;

/// <summary>Describes a named, durable, nonexclusive quorum queue and its failure topology.</summary>
public sealed class RabbitMqQueueOptions
{
    /// <summary>Gets the required queue name. Server-generated and amq.* names are not supported.</summary>
    public string Name { get; init; } = "";

    /// <summary>Gets the retry policy shared by all consumers of this queue.</summary>
    public RabbitMqRetryOptions Retry { get; init; } = new();

    /// <summary>Gets the dead-letter topology. Dead lettering uses at-least-once transfer and reject-publish overflow.</summary>
    public RabbitMqDeadLetterOptions DeadLetter { get; init; } = new();

    /// <summary>Gets the optional positive queue length limit, including delayed and pending dead-letter messages.</summary>
    public long? MaxLength { get; init; }

    /// <summary>Gets the optional positive total body size limit in bytes.</summary>
    public long? MaxLengthBytes { get; init; }

    /// <summary>Gets extra queue arguments. Arguments owned by this library cannot be overridden.</summary>
    public IReadOnlyDictionary<string, object?> Arguments { get; init; } = new Dictionary<string, object?>();
}
