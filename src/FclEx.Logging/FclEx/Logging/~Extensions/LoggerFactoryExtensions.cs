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

    public static ILogger CreateLoggerOrDefault(this ILoggerFactory? loggerFactory, Type type)
    {
        return loggerFactory.DefaultIfNull().CreateLogger(type);
    }

    public static ILogger<T> CreateLoggerOrDefault<T>(this ILoggerFactory? loggerFactory)
    {
        return loggerFactory.DefaultIfNull().CreateLogger<T>();
    }
}