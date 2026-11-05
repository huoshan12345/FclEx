using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

/// <summary>Fluent helpers for configuring exclusions, enrichers, sinks, exception formatting, and Microsoft logging registration.</summary>
public static class SerilogConfigurationExtensions
{
    /// <summary>Appends the current built-in exception, message, property, and source exclusion rules.</summary>
    /// <param name="configuration">The configuration receiving those rules.</param>
    /// <returns>The same configuration.</returns>
    /// <remarks>Rules are copied from the CommonItems arrays at this call. Repeated calls append them again.</remarks>
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

    /// <summary>Appends exclusion rules in enumeration order.</summary>
    /// <param name="configuration">The configuration receiving the rules.</param>
    /// <param name="excluders">Rules enumerated immediately, once. Matching any rule drops an event.</param>
    /// <returns>The same configuration.</returns>
    /// <remarks>If enumeration throws, rules already appended remain registered.</remarks>
    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, IEnumerable<ILogEventExcluder> excluders)
    {
        foreach (var excluder in excluders)
        {
            configuration.Exclude(excluder);
        }
        return configuration;
    }


    /// <summary>Registers a predicate whose true result excludes an event.</summary>
    /// <param name="configuration">The configuration receiving the predicate.</param>
    /// <param name="predicate">Evaluates events during filtering, before exception unwrapping and formatting.</param>
    /// <returns>The same configuration.</returns>
    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, Func<LogEvent, bool> predicate)
    {
        return configuration.Exclude(new LogEventExcluder(predicate));
    }

    /// <summary>Registers the same enricher instance on each logger created from this configuration.</summary>
    /// <param name="configuration">The configuration receiving the enricher.</param>
    /// <param name="enricher">An enricher suitable for concurrent logging.</param>
    /// <returns>The same configuration.</returns>
    public static SerilogConfiguration Enrich(this SerilogConfiguration configuration, ILogEventEnricher enricher)
    {
        return configuration.Configure(m => m.Enrich.With(enricher));
    }

    /// <summary>Registers an existing sink instance with each logger created from this configuration.</summary>
    /// <param name="configuration">The configuration receiving the sink registration.</param>
    /// <param name="sink">The sink whose disposal is transferred to each created logger.</param>
    /// <returns>The same configuration.</returns>
    /// <remarks>For independent loggers, create disposable sinks inside the WriteTo action overload instead of sharing this instance.</remarks>
    public static SerilogConfiguration WriteTo(this SerilogConfiguration configuration, ILogEventSink sink)
    {
        return configuration.Configure(m => m.WriteTo.Sink(sink));
    }

    /// <summary>Sets whether normalized exceptions are wrapped for consistent formatting.</summary>
    /// <param name="configuration">The configuration to update.</param>
    /// <param name="value">Whether to format exceptions; exception-level processing and duplicate-message unwrapping still run when false.</param>
    /// <returns>The same configuration.</returns>
    public static SerilogConfiguration FormatException(this SerilogConfiguration configuration, bool value)
    {
        configuration.FormatException = value;
        return configuration;
    }

    /// <summary>Appends a sink-configuration action to execute whenever a logger is created.</summary>
    /// <param name="configuration">The configuration receiving the action.</param>
    /// <param name="action">Configures sinks; create new disposable sink instances here for independent logger ownership.</param>
    /// <returns>The same configuration.</returns>
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
