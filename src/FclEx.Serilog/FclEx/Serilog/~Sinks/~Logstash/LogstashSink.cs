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

    public LogstashSink(LogstashSinkOptions options)
    {
        var uri = new Uri(options.Uri);
        var type = Enum.Parse(uri.Scheme, LogstashInputType.Tcp);
        _input = LogstashInputFactory.Create(type, uri);
        _formatter = options.Formatter ?? new JsonFormatter();
    }
}