namespace Microsoft.Extensions.Logging;

public static class LoggerProviderExtensions
{
    public static ILogger CreateLogger(this ILoggerProvider provider, Type t)
    {
        var name = t.LongName();
        return provider.CreateLogger(name);
    }

    public static ILogger CreateLogger<T>(this ILoggerProvider provider)
    {
        return provider.CreateLogger(typeof(T));
    }
}