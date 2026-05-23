namespace FclEx.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptionsInstance<TOptions>(this IServiceCollection services, Func<IServiceProvider, string, TOptions> factory)
        where TOptions : class
    {
        services.AddSingleton<IOptionsFactory<TOptions>>(m => new InstanceOptionsFactory<TOptions>(m, factory));
        return services;
    }

    public static IServiceCollection AddOptionsInstance<TOptions>(this IServiceCollection services, TOptions options)
        where TOptions : class
    {
        services.AddOptionsInstance((_, _) => options);
        return services;
    }

    public static IServiceCollection Configure<TOptions, TService>(this IServiceCollection services, Action<TOptions, TService> configureOptions)
        where TOptions : class
        where TService : class
    {
        services.AddOptions<TOptions>()
            .Configure(configureOptions);
        return services;
    }
}