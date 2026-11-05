using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class SerilogConfigurationExtensions
{
    public static SerilogConfiguration AddCommonExcluders(this SerilogConfiguration configuration)
    {
        return configuration
            .Exclude(ExceptionExcluder.CommonItems)
            .Exclude(MessageExcluder.CommonItems)
            .Exclude(PropertyExcluder.CommonItems)
            .Exclude(SourceExcluder.CommonItems);
    }

    /// <inheritdoc cref="ConfigureLogging"/>
    public static SerilogConfiguration ConfigureLoggingBuilder(this SerilogConfiguration configuration, Action<ILoggingBuilder> action)
    {
        return configuration.ConfigureLogging(action);
    }

    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, IEnumerable<ILogEventExcluder> excluders)
    {
        foreach (var excluder in excluders)
        {
            configuration.Exclude(excluder);
        }
        return configuration;
    }


    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, Func<LogEvent, bool> predicate)
    {
        return configuration.Exclude(new LogEventExcluder(predicate));
    }

    public static SerilogConfiguration Enrich(this SerilogConfiguration configuration, ILogEventEnricher enricher)
    {
        return configuration.Configure(m => m.Enrich.With(enricher));
    }

    public static SerilogConfiguration WriteTo(this SerilogConfiguration configuration, ILogEventSink sink)
    {
        return configuration.Configure(m => m.WriteTo.Sink(sink));
    }

    public static SerilogConfiguration FormatException(this SerilogConfiguration configuration, bool value)
    {
        configuration.FormatException = value;
        return configuration;
    }

    public static SerilogConfiguration WriteTo(this SerilogConfiguration configuration, Action<LoggerSinkConfiguration> action)
    {
        return configuration.Configure(m => action(m.WriteTo));
    }

    /// <summary>Appends an action to configure Microsoft's logging builder during service registration.</summary>
    /// <param name="configuration">The Serilog configuration that stores the action.</param>
    /// <param name="action">The action to run while Microsoft logging services are registered.</param>
    /// <returns>The same configuration for further customization.</returns>
    /// <remarks>
    /// Actions run in registration order when <see cref="ServiceCollectionExtensions.AddSerilog(IServiceCollection, SerilogConfiguration)"/>
    /// configures Microsoft's logging services, before deferred Serilog logger creation.
    /// This method and <see cref="ConfigureLoggingBuilder"/> share the same behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> or <paramref name="action"/> is null.</exception>
    public static SerilogConfiguration ConfigureLogging(this SerilogConfiguration configuration, Action<ILoggingBuilder> action)
    {
        Check.NotNull(configuration);
        Check.NotNull(action);
        configuration.LoggingBuilderConfigure += action;
        return configuration;
    }
}
