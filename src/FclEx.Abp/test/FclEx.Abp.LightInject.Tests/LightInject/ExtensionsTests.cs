namespace LightInject;

public class ExtensionsTests
{
    public class SingletonObj
    {
        public int Age { get; set; }
    }

    public class ScopeObj
    {
        public string? Name { get; set; }
    }

    public class TransientObj
    {
        public byte No { get; set; }
    }

    private static IServiceProvider Create()
    {
        var services = new ServiceCollection()
            .AddScoped<ScopeObj>()
            .AddTransient<TransientObj>()
            .AddSingleton<SingletonObj>();

        var container = LightInjectHelper.CreateContainer();
        container.RegisterMsDiService(services);

        var provider = container.GetInstance<IServiceProvider>();
        return provider;
    }

    [Fact]
    public void GetScopeObjOutsideTheScope()
    {
        var provider = Create();
        Assert.Throws<InvalidOperationException>(() => provider.GetService<ScopeObj>());
    }

    [Fact]
    public void GetScopeObjFromSameScope()
    {
        var provider = Create();
        using var scope = provider.CreateScope();
        var obj = scope.ServiceProvider.GetRequiredService<ScopeObj>();
        var obj2 = scope.ServiceProvider.GetRequiredService<ScopeObj>();
        Assert.Equal(obj, obj2);
    }

    [Fact]
    public void GetScopeObjFromDiffScope()
    {
        var provider = Create();
        using var outerScope = provider.CreateScope();
        var obj = outerScope.ServiceProvider.GetRequiredService<ScopeObj>();
        using var innerScope = provider.CreateScope();
        var obj2 = innerScope.ServiceProvider.GetRequiredService<ScopeObj>();
        Assert.NotEqual(obj, obj2);
    }

    [Fact]
    public void GetTransientObjFromSameScope()
    {
        var provider = Create();
        using var scope = provider.CreateScope();
        var obj = scope.ServiceProvider.GetRequiredService<TransientObj>();
        var obj2 = scope.ServiceProvider.GetRequiredService<TransientObj>();
        Assert.NotEqual(obj, obj2);
    }

    [Fact]
    public void GetTransientObjFromDiffScope()
    {
        var provider = Create();
        using var outerScope = provider.CreateScope();
        var obj = outerScope.ServiceProvider.GetRequiredService<TransientObj>();
        using var innerScope = provider.CreateScope();
        var obj2 = innerScope.ServiceProvider.GetRequiredService<TransientObj>();
        Assert.NotEqual(obj, obj2);
    }

    [Fact]
    public void GetSingletonObjFromSameScope()
    {
        var provider = Create();
        using var scope = provider.CreateScope();
        var obj = scope.ServiceProvider.GetRequiredService<SingletonObj>();
        var obj2 = scope.ServiceProvider.GetRequiredService<SingletonObj>();
        Assert.Equal(obj, obj2);
    }

    [Fact]
    public void GetSingletonObjFromDiffScope()
    {
        var provider = Create();
        using var outerScope = provider.CreateScope();
        var obj = outerScope.ServiceProvider.GetRequiredService<SingletonObj>();
        using var innerScope = provider.CreateScope();
        var obj2 = innerScope.ServiceProvider.GetRequiredService<SingletonObj>();
        Assert.Equal(obj, obj2);
    }
}