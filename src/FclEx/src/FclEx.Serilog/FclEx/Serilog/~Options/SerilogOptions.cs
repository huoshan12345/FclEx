namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class SerilogOptions
{
    public delegate void ConfigureAction(LoggerConfiguration configuration, SerilogOptions options);

    public List<ILogEventFilterItem> FilterItems { get; } = [];
    public List<ConfigureAction> ConfigureActions { get; } = [];
    public JsonFormatterOptions FormatterOptions { get; } = JsonFormatterOptions.Default;
    public SinkOptions DebugSink { get; set; } = SinkOptions.ConsoleText;
    public SinkOptions ReleaseSink { get; set; } = SinkOptions.ConsoleText;
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public Action<LogEvent>? LogEventModifier { get; set; }

    public SerilogOptions Configure(ConfigureAction action)
    {
        ConfigureActions.Add(action);
        return this;
    }

    public SerilogOptions Configure(Action<LoggerConfiguration> action)
    {
        return Configure((configuration, options) => action(configuration));
    }

    public const string DefaultOutputTemplate
        = "[{Timestamp:HH:mm:ss zzz} {Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}";

    public static readonly ConfigureAction SetLevel = (configuration, options) =>
    {
        configuration.MinimumLevel.Is(options.MinimumLevel);
    };

    public static readonly ConfigureAction Enrich = (configuration, options) =>
    {
        configuration
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext();
    };

    public static readonly ConfigureAction Filter = (configuration, options) =>
    {
        configuration.Filter.ByExcluding(m => options.FilterItems.Any(x => x.Match(m)));
    };

    public static readonly ConfigureAction WriteToConsole = (configuration, options) =>
    {
        configuration.WriteTo.Console(outputTemplate: DefaultOutputTemplate);
    };

    public static readonly ConfigureAction WriteToConsoleJson = (configuration, options) =>
    {
        configuration.WriteTo.Console(new JsonFormatter(options.FormatterOptions));
    };

    public static readonly ConfigureAction WriteToNewRelic = (configuration, options) =>
    {
        configuration.WriteTo.NewRelic(formatter: new JsonFormatter(options.FormatterOptions));
    };

    public static ConfigureAction Sink(SinkOptions options)
    {
        return (options.Sink, options.Format) switch
        {
            (SinkType.Console, FormatType.Text) => WriteToConsole,
            (SinkType.Console, FormatType.Json) => WriteToConsoleJson,
            (SinkType.NewRelic, FormatType.Json) => WriteToNewRelic,
            var (sink, format) => throw new NotSupportedException($"It is not supported to use '{sink}' sink with '{format}' format."),
        };
    }

    public static ConfigureAction Wrap(SinkOptions debugOptions, SinkOptions releaseOptions)
    {
        var debugAction = Sink(debugOptions);
        var releaseAction = Sink(releaseOptions);

        return (configuration, options) =>
        {
            var action = Debugger.IsAttached ? debugAction : releaseAction;
            configuration.WriteTo.Wrap(m => new LogEventSinkAdapter(m, options.LogEventModifier),
                c => action(configuration, options));
        };
    }

    public SerilogOptions()
    {
        FilterItems.AddRange(ExceptionFilterItem.CommonItems);
        FilterItems.AddRange(MessageFilterItem.CommonItems);
        FilterItems.AddRange(PropertyFilterItem.CommonItems);
        FilterItems.AddRange(SourceFilterItem.CommonItems);

        ConfigureActions.Add(SetLevel);
        ConfigureActions.Add(Enrich);
        ConfigureActions.Add(Filter);

        // NOTE: we use an action to wrap the method 'Wrap' here to lazily load DebugSink and ReleaseSink
        ConfigureActions.Add((configuration, options) => Wrap(DebugSink, ReleaseSink)(configuration, options));
    }
}