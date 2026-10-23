using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace FclEx.EfCore;

/// <summary>
/// Provides extensions for managing Entity Framework Core service registrations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Removes all registrations for <typeparamref name="TContext"/> and its typed options,
    /// including options configuration callbacks on EF Core 9 and later.
    /// </summary>
    /// <typeparam name="TContext">The concrete context type whose registrations are removed.</typeparam>
    /// <param name="services">The service collection to modify before building the service provider.</param>
    /// <returns>The same service collection, allowing calls to be chained.</returns>
    /// <remarks>
    /// Use this before registering the context again with different options or lifetimes.
    /// Missing registrations are ignored, and repeated calls are harmless.
    /// This method does not remove service aliases, context factories, pooling services, or
    /// non-generic <see cref="DbContextOptions"/> registrations. Those non-generic registrations
    /// can still reference the removed typed options and fail to resolve until the typed options
    /// are registered again. It does not affect existing service providers or dispose existing contexts.
    /// </remarks>
    public static IServiceCollection RemoveDbContext<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.RemoveAll<TContext>();
        services.RemoveAll<DbContextOptions<TContext>>();

#if NET9_0_OR_GREATER
        services.RemoveAll<IDbContextOptionsConfiguration<TContext>>();
#endif
        return services;
    }

    /// <summary>Registers a DbContext whose options are configured using a required service from dependency injection.</summary>
    /// <typeparam name="TContext">The context type to register.</typeparam>
    /// <typeparam name="TDependency">The service type resolved when EF constructs the options.</typeparam>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <param name="optionsAction">Configures the options using the resolved service; cannot be null.</param>
    /// <param name="contextLifetime">The context registration lifetime.</param>
    /// <param name="optionsLifetime">The options registration lifetime.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="ArgumentNullException">The configuration callback is null.</exception>
    /// <remarks>The dependency is resolved later from the provider constructing the options and must be registered
    /// with a compatible lifetime. Resolution and callback exceptions propagate when options are constructed.</remarks>
    public static IServiceCollection AddDbContextBy<TContext, TDependency>(
        this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder, TDependency> optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TDependency : notnull
    {
        Check.NotNull(optionsAction);
        return serviceCollection.AddDbContext<TContext>((sp, o) =>
        {
            var dependency = sp.GetRequiredService<TDependency>();
            optionsAction.Invoke(o, dependency);
        }, contextLifetime, optionsLifetime);
    }

    /// <summary>Registers a DbContext configured from the Value of a required IOptions&lt;TOptions&gt; service.</summary>
    /// <typeparam name="TContext">The context type to register.</typeparam>
    /// <typeparam name="TOptions">The configuration options type.</typeparam>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <param name="optionsAction">Configures the context options from the configuration value; cannot be null.</param>
    /// <param name="contextLifetime">The context registration lifetime.</param>
    /// <param name="optionsLifetime">The context options registration lifetime.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="ArgumentNullException">The configuration callback is null.</exception>
    /// <remarks>Uses IOptions, not IOptionsSnapshot or IOptionsMonitor. Options are resolved and validated when
    /// EF constructs its options. This method does not register or bind TOptions.</remarks>
    public static IServiceCollection AddDbContextByOptions<TContext, TOptions>(
        this IServiceCollection serviceCollection,
        Action<DbContextOptionsBuilder, TOptions> optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TContext : DbContext
        where TOptions : class
    {
        Check.NotNull(optionsAction);
        return serviceCollection.AddDbContextBy<TContext, IOptions<TOptions>>(
            (builder, options) => optionsAction.Invoke(builder, options.Value),
            contextLifetime,
            optionsLifetime);
    }
}
