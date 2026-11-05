namespace FclEx.Serilog;

public class LogstashSink : IBatchedLogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly ILogstashInput _input;

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> events)
    {
        var strings = events.Select(m => m.ToString(_formatter)).ToList();
        await _input.SendAsync(strings);
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>Creates a Logstash sink using the TCP or UDP endpoint in the supplied options.</summary>
    /// <param name="options">The endpoint URI and event formatter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    /// <exception cref="UriFormatException">The endpoint is not a valid absolute URI.</exception>
    /// <exception cref="ArgumentException">The URI does not identify a TCP or UDP host with an explicit port from 1 through 65535.</exception>
    public LogstashSink(LogstashSinkOptions options)
    {
        Check.NotNull(options);
        var uri = new Uri(options.Uri, UriKind.Absolute);
        if (uri.Host.IsNullOrEmpty() || uri.Port <= 0)
            throw new ArgumentException("Logstash requires a host and an explicit port from 1 through 65535.", nameof(options));

        _input = LogstashInputFactory.Create(uri);
        _formatter = options.Formatter ?? new JsonFormatter();
    }
}
