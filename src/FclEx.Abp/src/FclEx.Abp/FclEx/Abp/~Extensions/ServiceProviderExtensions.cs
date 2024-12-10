using System;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

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

    public static T GetOptions<T>(this IServiceProvider provider) where T : class, new()
    {
        return provider.GetRequiredService<IOptions<T>>().Value;
    }

    public static T? GetObject<T>(this IServiceProvider provider)
    {
        return provider.GetRequiredService<IObjectAccessor<T>>().Value;
    }
}