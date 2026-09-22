using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
}
