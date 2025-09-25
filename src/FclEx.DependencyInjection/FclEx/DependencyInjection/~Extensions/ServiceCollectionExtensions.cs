using FclEx.Extensions;

namespace FclEx.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection Add<T, TImpl>(this IServiceCollection services, ServiceLifetime lifetime)
        where T : class where TImpl : class, T
    {
        Check.NotNull(services);
        services.Add(new ServiceDescriptor(typeof(T), typeof(TImpl), lifetime));
        return services;
    }

    public static IServiceCollection TryAdd<T, TImpl>(this IServiceCollection services, ServiceLifetime lifetime)
        where T : class where TImpl : class, T
    {
        Check.NotNull(services);
        services.TryAdd(new ServiceDescriptor(typeof(T), typeof(TImpl), lifetime));
        return services;
    }

    public static IServiceCollection AddSingletonBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func) 
        where TDependency : notnull
        where T : class
    {
        return services.AddSingleton(s => func(s.GetRequiredService<TDependency>()));
    }

    public static IServiceCollection TryAddSingletonBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func) 
        where TDependency : notnull
        where T : class
    {
        services.TryAddSingleton(s => func(s.GetRequiredService<TDependency>()));
        return services;
    }

    public static IServiceCollection AddScopedBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func)
        where TDependency : notnull
        where T : class
    {
        return services.AddScoped(s => func(s.GetRequiredService<TDependency>()));
    }

    public static IServiceCollection TryAddScopedBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func)
        where TDependency : notnull
        where T : class
    {
        services.TryAddScoped(s => func(s.GetRequiredService<TDependency>()));
        return services;
    }

    public static IServiceCollection AddTransientBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func)
        where TDependency : notnull
        where T : class
    {
        return services.AddTransient(s => func(s.GetRequiredService<TDependency>()));
    }

    public static IServiceCollection TryAddTransientBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func)
        where TDependency : notnull
        where T : class
    {
        services.TryAddTransient(s => func(s.GetRequiredService<TDependency>()));
        return services;
    }

    public static IServiceCollection Replace<TService>(this IServiceCollection services, TService implementationInstance)
        where TService : class
    {
        services.RemoveAll(m => m.ServiceType == typeof(TService));
        services.AddSingleton<TService>(implementationInstance);
        return services;
    }

    public static IServiceCollection Remove(this IServiceCollection services, Func<ServiceDescriptor, bool> condition)
    {
        var toRemove = services.Where(condition).ToArray();
        toRemove.ForEach(m => services.Remove(m));
        return services;
    }

    public static IServiceCollection WrapFor<T>(this IServiceCollection services, Func<T, T> func,
        ServiceLifetime lifetime = ServiceLifetime.Singleton, Func<IServiceCollection, IServiceProvider>? builder = null) where T : notnull
    {
        var descriptor = services.FirstOrDefault(m => m.ServiceType == typeof(T));
        if (descriptor == null)
            throw new InvalidOperationException("There is no registered service of type: " + typeof(T).LongName());

        var provider = builder is null
            ? services.BuildServiceProvider()
            : builder(services);

        Func<IServiceProvider, object>? factory;
        switch (descriptor.Lifetime)
        {
            case ServiceLifetime.Singleton:
            {
                var instance = provider.GetRequiredService<T>();
                factory = _ => func(instance);
                break;
            }
            case ServiceLifetime.Scoped:
            {
                factory = _ =>
                {
                    using var scope = provider.CreateScope();
                    var instance = scope.ServiceProvider.GetRequiredService<T>();
                    return func(instance);
                };
                break;
            }
            case ServiceLifetime.Transient:
            {
                factory = _ =>
                {
                    var instance = provider.GetRequiredService<T>();
                    return func(instance)!;
                };
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(descriptor.Lifetime), descriptor.Lifetime, null);
        }
        services.Add(new ServiceDescriptor(typeof(T), factory, lifetime));
        return services;
    }
}