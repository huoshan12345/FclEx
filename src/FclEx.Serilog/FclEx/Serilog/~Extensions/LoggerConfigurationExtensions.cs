using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace FclEx.Serilog;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration WrapAllSinks(this LoggerConfiguration configuration, Func<ILogEventSink, ILogEventSink> wrapSink)
    {
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);

        for (var i = 0; i < sinks.Count; i++)
        {
            sinks[i] = wrapSink(sinks[i]);
        }

        return configuration;
    }

    private static readonly ConcurrentDictionary<(LoggerConfiguration, LoggerFilterOptions, string), LogEventLevel> _levelCache = [];

    /// <summary>Filters events using provider-independent Microsoft logging level rules.</summary>
    /// <param name="config">The Serilog configuration receiving the filter.</param>
    /// <param name="configuration">The Logging configuration section containing LogLevel rules.</param>
    /// <returns>The same Serilog configuration.</returns>
    /// <remarks>
    /// Configuration is read once when this method is called. Categories use case-insensitive prefix
    /// matching and support one wildcard. The longest matching category wins, with later rules winning ties.
    /// Serilog's minimum level determines which events reach this filter.
    /// </remarks>
    /// <exception cref="InvalidOperationException">A configured log level is invalid or a category contains multiple wildcards.</exception>
    public static LoggerConfiguration ApplyMicrosoftLoggingFilter(this LoggerConfiguration config, IConfiguration configuration)
    {
        var options = CreateLoggerFilterOptions(configuration);

        config.Filter.ByIncludingOnly(e =>
        {
            var category = e.GetSourceContext() ?? "";
            var minLevel = _levelCache.GetOrAdd((config, options, category), m => GetLogLevel(m.Item2, m.Item3));
            return e.Level >= minLevel;
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
