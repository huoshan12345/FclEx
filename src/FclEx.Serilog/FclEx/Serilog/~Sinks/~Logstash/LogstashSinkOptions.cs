namespace FclEx.Serilog;

/// <summary>Configures Logstash transport, formatting, and optional Serilog batching registration.</summary>
/// <remarks>Direct LogstashSink construction uses endpoint, formatter, and timeout settings; batching and level settings are applied by WriteTo.Logstash.</remarks>
public class LogstashSinkOptions
{
    /// <summary>Configures a Logstash endpoint and its batching defaults.</summary>
    /// <param name="uri">An absolute tcp:// or udp:// URI containing a host and explicit destination port.</param>
    /// <remarks>The sink validates the endpoint when it is constructed.</remarks>
    public LogstashSinkOptions(string uri)
    {
        Check.NotEmpty(uri);
        Uri = uri;
    }

    /// <summary>The TCP or UDP endpoint URI, including its host and explicit port.</summary>
    public string Uri { get; }

    /// <summary>
    /// The maximum number of events in a registered Serilog batch. Defaults to 10.
    /// </summary>
    public int BatchSizeLimit { get; set; } = 10;

    /// <summary>
    /// The registered batching buffer interval. Defaults to three seconds.
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>The total TCP connection and send deadline for one serialized batch. Defaults to 30 seconds.</summary>
    /// <remarks>
    /// Must be at least one millisecond and at most Int32.MaxValue milliseconds. Does not apply to UDP.
    /// Expiry closes the TCP connection and raises TimeoutException so batching can retry.
    /// A partially sent batch may be delivered again on retry. Formatting time is outside this deadline.
    /// </remarks>
    public TimeSpan TcpSendTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The maximum number of events that will be held in-memory while waiting to ship them to Logstash.
    /// Beyond this limit, events will be dropped. The default is 100,000.
    /// Applies to WriteTo.Logstash registration; direct EmitBatchAsync calls are not queued by this option.
    /// </summary>
    public int QueueLimit { get; set; } = 100000;

    /// <summary>
    /// Formats each event before transport. Null selects a new JsonFormatter.
    /// The formatter must match the receiving Logstash codec; the default produces JSON objects.
    /// </summary>
    public ITextFormatter? Formatter { get; set; }

    /// <summary>
    /// The minimum log event level required in order to write an event to the sink.
    /// Null allows any level. Ignored when LevelSwitch is specified; used by WriteTo.Logstash registration.
    /// </summary>
    public LogEventLevel? MinimumLogEventLevel { get; set; }

    /// <summary>
    /// A live switch controlling the registered sink's minimum level. Null uses MinimumLogEventLevel instead.
    /// </summary>
    public LoggingLevelSwitch? LevelSwitch { get; set; }
}
