using System.Collections.Generic;
using System.IO;
using FclEx.Serilog.Models;
using Serilog.Events;
using Serilog.Formatting;

namespace FclEx.Serilog.Formatting;

public class AbpJsonFormatter : ITextFormatter
{
    private readonly IList<string>? _excludePaths;

    public AbpJsonFormatter(IList<string>? excludePaths = null)
    {
        _excludePaths = excludePaths;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var abpLogEvent = AbpLogEvent.Create(logEvent, _excludePaths);
        output.Write(abpLogEvent.ToString(Newtonsoft.Json.Formatting.None));
    }
}