using System;

namespace FclEx.Abp;

public static class ServiceProviderExtensions
{
    public static IServiceProvider UseAbp(this IServiceProvider provider)
    {
        provider.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().Initialize(provider);
        return provider;
    }

    public static async Task<IServiceProvider> UseAbpAsync(this IServiceProvider provider)
    {
        await provider.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().InitializeAsync(provider);
        return provider;
    }

    public static async Task CloseAbpAsync(this IServiceProvider provider, bool dispose = true)
    {
        var app = provider.GetRequiredService<IAbpApplicationWithExternalServiceProvider>();
        await app.ShutdownAsync();

        if (dispose)
            app.Dispose();
    }

    public static T? GetObject<T>(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IObjectAccessor<T>>().Value;
    }
}