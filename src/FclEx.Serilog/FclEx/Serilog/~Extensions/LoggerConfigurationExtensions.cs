using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace FclEx.Serilog;

/// <summary>Wraps registered Serilog sinks and applies provider-independent Microsoft logging filters.</summary>
public static class LoggerConfigurationExtensions
{
    /// <summary>Wraps all currently registered WriteTo sinks as one pipeline, invoking the wrapper once per event before fan-out.</summary>
    /// <param name="configuration">The logger configuration whose existing WriteTo sinks will be replaced.</param>
    /// <param name="wrapSink">Creates a wrapper that owns the aggregate of captured sinks.</param>
    /// <returns>The same logger configuration.</returns>
    /// <remarks>
    /// Call after all sink registrations and before CreateLogger. AuditTo sinks and sinks registered later
    /// are outside this wrapper. Global event filters run before the wrapper; individual sink restrictions
    /// run after it. The wrapper must forward disposal, asynchronous disposal when needed, and failure
    /// notifications to its aggregate sink. LogEventMutateSink fulfills this contract.
    /// If wrapper construction fails, captured sinks are released and removed from the configuration.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    /// <exception cref="InvalidOperationException">The wrapper factory returns null.</exception>
    /// <exception cref="AggregateException">Wrapper construction and cleanup both fail.</exception>
    public static LoggerConfiguration WrapSinkPipeline(this LoggerConfiguration configuration, Func<ILogEventSink, ILogEventSink> wrapSink)
    {
        Check.NotNull(configuration);
        Check.NotNull(wrapSink);
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);
        var captured = sinks.ToArray();

        // Transfer ownership before wrapping: failure cleanup must not leave disposed sinks registered here.
        sinks.Clear();
        return configuration.WriteTo.Wrap(wrapSink, wrapped =>
        {
            foreach (var sink in captured)
                wrapped.Sink(sink);
        });
    }

    /// <summary>Replaces each currently registered WriteTo sink with its own wrapper.</summary>
    /// <param name="configuration">The configuration whose WriteTo registrations are updated.</param>
    /// <param name="wrapSink">Creates an owning wrapper for each existing sink, including any per-sink restriction wrapper.</param>
    /// <returns>The same configuration.</returns>
    /// <remarks>
    /// AuditTo sinks and later registrations are unaffected. Wrappers must forward disposal and failure
    /// notifications themselves. Unlike WrapSinkPipeline, processing happens separately for each sink.
    /// If the factory throws, earlier replacements remain and later sinks retain their original registrations.
    /// </remarks>
    public static LoggerConfiguration WrapAllSinks(this LoggerConfiguration configuration, Func<ILogEventSink, ILogEventSink> wrapSink)
    {
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);

        for (var i = 0; i < sinks.Count; i++)
        {
            sinks[i] = wrapSink(sinks[i]);
        }

        return configuration;
    }

    /// <summary>Filters events using provider-independent Microsoft logging level rules.</summary>
    /// <param name="config">The Serilog configuration receiving the filter.</param>
    /// <param name="configuration">The Logging configuration section containing LogLevel rules.</param>
    /// <returns>The same Serilog configuration.</returns>
    /// <remarks>
    /// Configuration is read once when this method is called. Categories use case-insensitive prefix
    /// matching and support one wildcard. The longest matching category wins, with later rules winning ties.
    /// Each filter owns its configuration snapshot and category-level cache.
    /// Serilog's minimum level determines which events reach this filter.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A configured log level is invalid or a category contains multiple wildcards.</exception>
    public static LoggerConfiguration ApplyMicrosoftLoggingFilter(this LoggerConfiguration config, IConfiguration configuration)
    {
        var options = CreateLoggerFilterOptions(configuration);

        // Keep the cache with this filter so it can be collected with the logger.
        var levels = new ConcurrentDictionary<string, LogEventLevel>(StringComparer.OrdinalIgnoreCase);

        // Reuse the factory delegate for cache misses and hits.
        Func<string, LogEventLevel> resolveLevel = category => GetLogLevel(options, category);

        config.Filter.ByIncludingOnly(logEvent =>
        {
            var category = logEvent.GetSourceContext() ?? "";
            var minimumLevel = levels.GetOrAdd(category, resolveLevel);
            return logEvent.Level >= minimumLevel;
        });

        return config;
    }

    // it is in Microsoft.Extensions.Logging.Configuration
    private static readonly Type LoggerFilterConfigureOptionsType = typeof(LoggerProviderOptions).Assembly
        .GetRequiredType("Microsoft.Extensions.Logging.LoggerFilterConfigureOptions");

    private static LoggerFilterOptions CreateLoggerFilterOptions(IConfiguration configuration)
    {
        var configure = Activator.CreateInstance(LoggerFilterConfigureOptionsType, configuration)
            .CastTo<IConfigureOptions<LoggerFilterOptions>>()!;

        var factory = new OptionsFactory<LoggerFilterOptions>([configure], []);
        var options = factory.Create("");
        foreach (var rule in options.Rules)
        {
            if (rule.ProviderName == null && rule.CategoryName is { } category
                && category.IndexOf('*') != category.LastIndexOf('*'))
                throw new InvalidOperationException("Only one wildcard character is allowed in a category name.");
        }
        return options;
    }

    private static LogEventLevel GetLogLevel(LoggerFilterOptions options, string category)
    {
        LoggerFilterRule? best = null;
        foreach (var rule in options.Rules)
        {
            if (rule.ProviderName != null)
                continue;

            if (rule.CategoryName is { } ruleCategory && !MatchesCategory(ruleCategory, category))
                continue;

            if (best?.CategoryName is { } bestCategory
                && (rule.CategoryName == null || bestCategory.Length > rule.CategoryName.Length))
                continue;

            best = rule;
        }

        var minLevel = best?.LogLevel ?? options.MinLevel;

        return minLevel.ToSerilogLevel();
    }

    private static bool MatchesCategory(string ruleCategory, string category)
    {
        var wildcardIndex = ruleCategory.IndexOf('*');
        if (wildcardIndex < 0)
            return category.StartsWith(ruleCategory, StringComparison.OrdinalIgnoreCase);

        return category.StartsWith(ruleCategory[..wildcardIndex], StringComparison.OrdinalIgnoreCase)
            && category.EndsWith(ruleCategory[(wildcardIndex + 1)..], StringComparison.OrdinalIgnoreCase);
    }
}
