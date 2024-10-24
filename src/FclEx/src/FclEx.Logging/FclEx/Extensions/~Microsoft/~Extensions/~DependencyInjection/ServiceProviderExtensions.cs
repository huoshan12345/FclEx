namespace FclEx.Extensions;

public static class ServiceProviderExtensions
{
    public static T GetServiceOr<T>(this IServiceProvider provider, T defaultValue) where T : class
        => provider.GetService<T>() ?? defaultValue;

    public static T GetServiceOr<T>(this IServiceProvider provider, Func<IServiceProvider, T> factory) where T : class
        => provider.GetService<T>() ?? factory(provider);

    public static ILoggerFactory GetLoggerFactory(this IServiceProvider resolver)
        => resolver.GetServiceOr<ILoggerFactory>(NullLoggerFactory.Instance);

    public static ILogger CreateLogger(this IServiceProvider provider, string name)
        => provider.GetLoggerFactory().CreateLogger(name);

    public static ILogger CreateLogger(this IServiceProvider provider, Type type)
        => provider.GetLoggerFactory().CreateLogger(type);

    public static ILogger<T> CreateLogger<T>(this IServiceProvider provider)
        => provider.GetLoggerFactory().CreateLogger<T>();
}