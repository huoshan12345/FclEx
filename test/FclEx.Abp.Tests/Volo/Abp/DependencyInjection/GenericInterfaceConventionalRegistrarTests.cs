namespace Volo.Abp.DependencyInjection;

public class GenericInterfaceConventionalRegistrarTests(AbpTestsFixture fixture) : AbpTests(fixture)
{
    public interface IGenericSingleton<out T> : ISingletonDependency;
    public class GenericSingleton : IGenericSingleton<string>;

    public interface IGenericTransient<T> : ITransientDependency;
    public class GenericTransient : IGenericTransient<string>;

    [Fact]
    public void GenericSingleton_Test()
    {
        var obj = Services.GetRequiredService<IGenericSingleton<string>>();
        var obj2 = Services.GetRequiredService<IGenericSingleton<string>>();
        var obj3 = Services.GetRequiredService<GenericSingleton>();
        Assert.Equal(obj, obj2);
        Assert.Equal(obj, obj3);
    }

    [Fact]
    public void GenericTransient_Test()
    {
        var obj = Services.GetRequiredService<IGenericTransient<string>>();
        var obj2 = Services.GetRequiredService<IGenericTransient<string>>();
        var obj3 = Services.GetRequiredService<GenericTransient>();
        Assert.NotEqual(obj, obj2);
        Assert.NotEqual(obj, obj3);
        Assert.NotEqual(obj2, obj3);
    }
}