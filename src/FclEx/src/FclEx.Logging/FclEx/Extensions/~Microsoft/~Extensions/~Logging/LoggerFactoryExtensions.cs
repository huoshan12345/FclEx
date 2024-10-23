namespace FclEx.Extensions;

public static class LoggerFactoryExtensions
{
    private const string LoggingAssemblyName = "Microsoft.Extensions.Logging";
    private static readonly Type? LoggerFactory = TypeHelper.GetType("Microsoft.Extensions.Logging.LoggerFactory", LoggingAssemblyName);
    private static readonly FieldInfo? FilterOptions = LoggerFactory?.GetRequiredField("_filterOptions");
    private static readonly Type? LoggerFilterOptions = TypeHelper.GetType("Microsoft.Extensions.Logging.LoggerFilterOptions", LoggingAssemblyName);
    private static readonly PropertyInfo? MinLevel = LoggerFilterOptions?.GetRequiredProperty("MinLevel");

    public static void SetMinimumLevel(this ILoggerFactory factory, LogLevel minLevel)
    {
        Check.NotNull(factory);

        if (factory.GetType().IsAssignableTo(LoggerFactory))
        {
            var options = FilterOptions?.GetRequiredValue(factory);
            MinLevel?.SetValue(options, minLevel);
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