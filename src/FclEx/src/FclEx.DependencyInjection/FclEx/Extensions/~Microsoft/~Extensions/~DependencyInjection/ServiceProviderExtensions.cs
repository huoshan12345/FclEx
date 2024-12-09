namespace FclEx.Extensions;

public static class ServiceProviderExtensions
{
    public static T GetServiceOr<T>(this IServiceProvider provider, T defaultValue) where T : class
        => provider.GetService<T>() ?? defaultValue;

    public static T GetServiceOr<T>(this IServiceProvider provider, Func<IServiceProvider, T> factory) where T : class
        => provider.GetService<T>() ?? factory(provider);
}