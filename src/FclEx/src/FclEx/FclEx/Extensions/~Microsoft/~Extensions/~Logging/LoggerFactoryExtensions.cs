namespace FclEx.Extensions;

public static class LoggerFactoryExtensions
{
    private static readonly FieldInfo FieldOfFilterOptions = typeof(LoggerFactory).GetRequiredField("_filterOptions");

    public static void SetMinimumLevel(this ILoggerFactory factory, LogLevel minLevel)
    {
        Check.NotNull(factory);

        if (factory is LoggerFactory fac)
        {
            var options = FieldOfFilterOptions.GetRequiredValue<LoggerFilterOptions>(fac);
            options.MinLevel = minLevel;
        }
        else
        {
            throw new NotSupportedException("Not supported logger factory type: " + factory.GetType().LongName());
        }
    }

    public static ILoggerFactory Touch(this ILoggerFactory? factory)
    {
        return factory ?? NullLoggerFactory.Instance;
    }
}