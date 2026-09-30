namespace FclEx.Logging;

public static class LoggerFactoryExtensions
{
    private static readonly FieldInfo FilterOptions = typeof(LoggerFactory).GetRequiredField("_filterOptions");

    public static void SetMinimumLevel(this ILoggerFactory factory, LogLevel minLevel)
    {
        Check.NotNull(factory);

        if (factory is LoggerFactory loggerFactory)
        {
            var options = FilterOptions.GetRequiredValue<LoggerFilterOptions>(loggerFactory);
            options.MinLevel = minLevel;
        }
        else
        {
            throw new NotSupportedException("Not supported logger factory type: " + factory.GetType().LongName());
        }
    }

    public static ILoggerFactory DefaultIfNull(this ILoggerFactory? factory)
    {
        return factory ?? NullLoggerFactory.Instance;
    }

    public static ILogger CreateLoggerOrDefault(this ILoggerFactory? factory, Type type)
    {
        return factory.DefaultIfNull().CreateLogger(type);
    }

    public static ILogger<T> CreateLoggerOrDefault<T>(this ILoggerFactory? factory)
    {
        return factory.DefaultIfNull().CreateLogger<T>();
    }

    public static CollectingLoggerProvider AddCollecting(this ILoggerFactory factory)
    {
        Check.NotNull(factory);

        var provider = new CollectingLoggerProvider();
        factory.AddProvider(provider);
        return provider;
    }
}