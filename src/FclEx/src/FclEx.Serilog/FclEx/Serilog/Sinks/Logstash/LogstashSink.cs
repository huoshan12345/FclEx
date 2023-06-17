using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Extensions;
using FclEx.Serilog.Formatting;
using FclEx.Serilog.Sinks.Logstash.Inputs;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace FclEx.Serilog.Sinks.Logstash;

public class LogstashSink : IBatchedLogEventSink
{
    private readonly ITextFormatter _formatter;
    private readonly ILogstashInput _input;

    public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        var strs = events.Select(m => m.ToString(_formatter)).ToList();
        await _input.SendAsync(strs).DonotCapture();
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
        _formatter = options.Formatter ?? new LogstashJsonFormatter();
    }
}