using System.Threading;

namespace FclEx.Kafka;

/// <summary>Configures the topic and callbacks for a sequential Kafka consumer.</summary>
/// <typeparam name="T">The deserialized value type, which may include null tombstones.</typeparam>
public class KafkaConsumerOptions<T>
{
    /// <summary>Gets or sets the application-defined consumer name required when starting the loop.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the handler whose successful completion permits committing the record's offset.</summary>
    /// <remarks>
    /// Null values can represent tombstones. A failure stops consumption without committing the failed record.
    /// Handlers that need to interrupt in-flight work must observe the configured cancellation token themselves.
    /// </remarks>
    public Func<T, Task>? MessageHandler { get; set; }

    /// <summary>Gets or sets an optional callback that reports a message-handler failure before consumption stops.</summary>
    /// <remarks>
    /// Completing this callback does not mark the record as processed or permit its offset to be committed.
    /// Callback failures are logged; the consumption task retains the original message-handler exception.
    /// This callback is not invoked for polling, deserialization, commit failures, or requested cancellation.
    /// </remarks>
    public Func<T, Exception, Task>? ErrorHandler { get; set; }

    /// <summary>Gets or sets the single topic to subscribe to.</summary>
    public string? Topic { get; set; }

    /// <summary>Gets or sets the logger, or null to disable logging.</summary>
    public ILogger? Logger { get; set; }

    /// <summary>Gets or sets the token that interrupts polling and stops the loop between records.</summary>
    /// <remarks>A handler that throws <see cref="OperationCanceledException"/> after cancellation stops the loop without committing that record.</remarks>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Gets or sets the value deserializer, or null to use the client's built-in deserializer for the value type.</summary>
    public IDeserializer<T>? Deserializer { get; set; } = new NewtonsoftJsonDeserializer<T>();
}
