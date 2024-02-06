using Volo.Abp.DependencyInjection;

namespace FclEx.Abp.DependencyInjection;

public class GenericInterfaceConventionalRegistrarTests : AbpTests<AbpTestModule>
{
    public GenericInterfaceConventionalRegistrarTests(ITestOutputHelper output, Action<AbpTestsOptions>? action = null)
        : base(output, action)
    {
    }

    public interface IGenericSingleton<out T> : ISingletonDependency { }
    public class GenericSingleton : IGenericSingleton<string> { }

    public interface IGenericTransient<T> : ITransientDependency { }
    public class GenericTransient : IGenericTransient<string> { }

    [Fact]
    public void GenericSingleton_Test()
    {
        var obj = ServiceProvider.GetRequiredService<IGenericSingleton<string>>();
        var obj2 = ServiceProvider.GetRequiredService<IGenericSingleton<string>>();
        var obj3 = ServiceProvider.GetRequiredService<GenericSingleton>();
        Assert.Equal(obj, obj2);
        Assert.Equal(obj, obj3);
    }

    [Fact]
    public void GenericTransient_Test()
    {
        var obj = ServiceProvider.GetRequiredService<IGenericTransient<string>>();
        var obj2 = ServiceProvider.GetRequiredService<IGenericTransient<string>>();
        var obj3 = ServiceProvider.GetRequiredService<GenericTransient>();
        Assert.NotEqual(obj, obj2);
        Assert.NotEqual(obj, obj3);
        Assert.NotEqual(obj2, obj3);
    }
}