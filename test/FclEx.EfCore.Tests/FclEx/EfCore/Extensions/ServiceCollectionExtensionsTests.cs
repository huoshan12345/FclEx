using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FclEx.EfCore.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void RemoveDbContext_RemovesAllContextAndOptionsRegistrations()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FirstContext>();
        services.AddTransient<FirstContext>();
        services.AddSingleton(new DbContextOptions<FirstContext>());
#if NET9_0_OR_GREATER
        services.ConfigureDbContext<FirstContext>(options => options.EnableDetailedErrors());
        services.ConfigureDbContext<FirstContext>(options => options.EnableSensitiveDataLogging());
#endif

        var result = services.RemoveDbContext<FirstContext>();

        Assert.Same(services, result);
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(FirstContext));
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(DbContextOptions<FirstContext>));
#if NET9_0_OR_GREATER
        Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<FirstContext>));
#endif
        using var provider = services.BuildServiceProvider();
        Assert.Null(provider.GetService<FirstContext>());
    }

    [Fact]
    public void RemoveDbContext_PreservesOtherServicesAndContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FirstContext>();
        services.AddDbContext<SecondContext>(options => options.UseSqlite("Data Source=other.db"));
        var marker = new object();
        services.AddSingleton(marker);
        var preserved = services.Where(descriptor => descriptor.ServiceType == typeof(SecondContext)
            || descriptor.ServiceType == typeof(DbContextOptions<SecondContext>)
            || descriptor.ServiceType == typeof(object)
#if NET9_0_OR_GREATER
            || descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<SecondContext>)
#endif
        ).ToArray();

        services.RemoveDbContext<FirstContext>();

        Assert.All(preserved, descriptor => Assert.Contains(descriptor, services));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Same(marker, scope.ServiceProvider.GetRequiredService<object>());
        var context = scope.ServiceProvider.GetRequiredService<SecondContext>();
        Assert.Equal("Data Source=other.db", context.Database.GetConnectionString());
    }

    [Fact]
    public void RemoveDbContext_IsNoOpWhenAbsentAndCanBeRepeated()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.RemoveDbContext<FirstContext>());
        Assert.Empty(services);

        services.AddDbContext<FirstContext>();
        services.RemoveDbContext<FirstContext>();
        var remaining = services.ToArray();
        Assert.Same(services, services.RemoveDbContext<FirstContext>());
        Assert.Equal(remaining, services.ToArray());
    }

    [Theory]
    [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
    [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
    public void RemoveDbContext_AllowsReregistrationWithNewProviderAndLifetime(
        ServiceLifetime originalLifetime,
        ServiceLifetime replacementLifetime)
    {
        var services = new ServiceCollection();
        services.AddDbContext<FirstContext>(options => options.UseSqlServer("Server=old;Database=old"),
            originalLifetime, originalLifetime);

        services.RemoveDbContext<FirstContext>();
        services.AddDbContext<FirstContext>(options => options.UseSqlite("Data Source=replacement.db"),
            replacementLifetime, replacementLifetime);

        Assert.Equal(replacementLifetime,
            Assert.Single(services, descriptor => descriptor.ServiceType == typeof(FirstContext)).Lifetime);
        Assert.Equal(replacementLifetime,
            Assert.Single(services, descriptor => descriptor.ServiceType == typeof(DbContextOptions<FirstContext>)).Lifetime);
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FirstContext>();
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.Equal("Data Source=replacement.db", context.Database.GetConnectionString());
    }

    private sealed class FirstContext(DbContextOptions<FirstContext> options) : DbContext(options);

    private sealed class SecondContext(DbContextOptions<SecondContext> options) : DbContext(options);
}
