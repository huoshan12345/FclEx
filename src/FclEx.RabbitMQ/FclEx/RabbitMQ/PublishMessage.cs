namespace FclEx.RabbitMQ;

/// <summary>A publication envelope with a stable ID and copied AMQP metadata.</summary>
/// <typeparam name="T">The value serialized as the message body.</typeparam>
/// <remarks>The body is retained by reference. Header values are recursively copied; mutating a returned header copy does not change the envelope.</remarks>
public sealed class PublishMessage<T>
{
    private readonly Dictionary<string, object?> _headers;

    /// <summary>Creates a publication. Generates an ID once when no ID is supplied.</summary>
    /// <param name="body">The value, including null when the serializer permits it.</param>
    /// <param name="options">Routing and scheduling options, or defaults for immediate publication with an empty routing key.</param>
    /// <param name="messageId">A stable nonempty identity, or null to generate one.</param>
    /// <param name="correlationId">An optional nonempty correlation identity.</param>
    /// <param name="headers">Supported AMQP field values; scheduling, retry and broker-owned keys are forbidden.</param>
    public PublishMessage(T body, RabbitMqPublishOptions? options = null, string? messageId = null,
        string? correlationId = null, IReadOnlyDictionary<string, object?>? headers = null)
    {
        Body = body;
        Options = OptionsSnapshot.Publish(options ?? new());
        MessageId = OptionsSnapshot.ShortString(messageId ?? Guid.NewGuid().ToString("N"), nameof(messageId), false);
        CorrelationId = correlationId is null ? null : OptionsSnapshot.ShortString(correlationId, nameof(correlationId), false);
        _headers = HeaderSnapshot.Copy(headers, rejectReserved: true);
    }

    /// <summary>Gets the body retained by reference.</summary>
    public T Body { get; }
    /// <summary>Gets the validated routing and scheduling settings.</summary>
    public RabbitMqPublishOptions Options { get; }
    /// <summary>Gets the identity reused when this envelope is published again.</summary>
    public string MessageId { get; }
    /// <summary>Gets the optional correlation identity.</summary>
    public string? CorrelationId { get; }
    /// <summary>Gets an independent, read-only field-table snapshot.</summary>
    public IReadOnlyDictionary<string, object?> Headers => HeaderSnapshot.ReadOnly(_headers);
    internal Dictionary<string, object?> CopyHeaders() => HeaderSnapshot.Copy(_headers);
}
