using System.Collections.Concurrent;
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

    private static readonly ConcurrentDictionary<string, LogEventLevel> _levelCache = [];

    public static LoggerConfiguration ApplyMicrosoftLoggingFilter(this LoggerConfiguration config, IConfiguration configuration)
    {
        var options = CreateLoggerFilterOptions(configuration);

        config.Filter.ByIncludingOnly(e =>
        {
            if (!e.Properties.TryGetValue(Constants.SourceContext, out var ctx))
                return true;

            var category = (string?)((ScalarValue)ctx).Value ?? "";
            var minLevel = _levelCache.GetOrAdd(category, m => GetLogLevel(options, m));
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
        return factory.Create("");
    }

    private static LogEventLevel GetLogLevel(LoggerFilterOptions options, string category)
    {
        LoggerFilterRule? best = null;
        var bestLength = -1;

        foreach (var rule in options.Rules)
        {
            if (rule.ProviderName != null)
                continue; // Serilog does not support provider name filtering, so skip rules that specify a provider name

            if (rule.CategoryName == null)
            {
                if (bestLength < 0)
                {
                    best = rule;
                    bestLength = 0;
                }

                continue;
            }

            if (category.StartsWith(rule.CategoryName, StringComparison.Ordinal) == false)
                continue;

            if (rule.CategoryName.Length <= bestLength)
                continue;

            best = rule;
            bestLength = rule.CategoryName.Length;
        }

        var minLevel = best?.LogLevel ?? options.MinLevel;

        return minLevel.ToSerilogLevel();
    }
}