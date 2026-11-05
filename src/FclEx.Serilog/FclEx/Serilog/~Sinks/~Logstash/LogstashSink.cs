namespace FclEx.Serilog;

/// <summary>Formats batches and sends newline-delimited events through a fresh TCP or UDP transport per batch.</summary>
public class LogstashSink : IBatchedLogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly ILogstashInput _input;

    /// <summary>Formats and sends a batch through a new TCP or UDP connection.</summary>
    /// <param name="events">The events to serialize and send in collection order.</param>
    /// <returns>An operation that completes when the batch has been sent to the transport.</returns>
    /// <remarks>
    /// Formatting and transport failures propagate to Serilog batching for diagnostics and retry.
    /// A successful send does not acknowledge server ingestion; retrying a partially sent batch can duplicate events.
    /// </remarks>
    /// <exception cref="TimeoutException">TCP connection and sending exceed the configured batch deadline.</exception>
    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        var strings = events.Select(m => m.ToString(_formatter)).ToList();
        await _input.SendAsync(strings);
    }

    /// <summary>Completes without opening a transport when batching has no events.</summary>
    /// <returns>An already-completed task.</returns>
    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>Creates a Logstash sink using the TCP or UDP endpoint in the supplied options.</summary>
    /// <param name="options">The endpoint URI, event formatter, and TCP send deadline, captured at construction.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    /// <exception cref="UriFormatException">The endpoint is not a valid absolute URI.</exception>
    /// <exception cref="ArgumentException">The URI does not identify a TCP or UDP host with an explicit port from 1 through 65535.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The TCP send timeout is outside the supported range.</exception>
    public LogstashSink(LogstashSinkOptions options)
    {
        Check.NotNull(options);
        var uri = new Uri(options.Uri, UriKind.Absolute);
        if (uri.Host.IsNullOrEmpty() || uri.Port <= 0)
            throw new ArgumentException("Logstash requires a host and an explicit port from 1 through 65535.", nameof(options));

        if (options.TcpSendTimeout < TimeSpan.FromMilliseconds(1) || options.TcpSendTimeout.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(options.TcpSendTimeout), options.TcpSendTimeout,
                "The TCP send timeout must be between one and Int32.MaxValue milliseconds.");

        _input = LogstashInputFactory.Create(uri, options.TcpSendTimeout);
        _formatter = options.Formatter ?? new JsonFormatter();
    }
}
