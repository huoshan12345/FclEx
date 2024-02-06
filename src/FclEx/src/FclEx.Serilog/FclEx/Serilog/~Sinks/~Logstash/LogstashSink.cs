namespace FclEx.Serilog;

public class LogstashSink : IBatchedLogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly ILogstashInput _input;

    public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        var strs = events.Select(m => m.ToString(_formatter)).ToList();
        await _input.SendAsync(strs).IgnoreSyncContext();
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    public LogstashSink(LogstashSinkOptions options)
    {
        var uri = new Uri(options.Uri);
        var type = uri.Scheme.ToEnum(LogstashInputType.Tcp);
        _input = LogstashInputFactory.Create(type, uri);
        _formatter = options.Formatter ?? new JsonFormatter();
    }
}