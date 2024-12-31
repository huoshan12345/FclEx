using FclEx.Abp;

namespace Volo.Abp.DependencyInjection;

public class OpenGenericConventionalRegistrarTests : AbpTests<AbpTestModule>
{
    public interface IGenericSingleton<out T> : ISingletonDependency;
    public class GenericSingleton<T> : IGenericSingleton<T>;

    public interface IGenericTransient<T> : ITransientDependency;
    public class GenericTransient<T> : IGenericTransient<T>;

    public interface INonGenericTransient : ITransientDependency;
    public class NonGenericTransient<T> : INonGenericTransient;

    public OpenGenericConventionalRegistrarTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void GenericSingleton_Test()
    {
        Test<int>();
        Test<string>();

        void Test<T>()
        {
            var obj = ServiceProvider.GetRequiredService<IGenericSingleton<T>>();
            var obj2 = ServiceProvider.GetRequiredService<IGenericSingleton<T>>();
            var obj3 = ServiceProvider.GetRequiredService<GenericSingleton<T>>();
            Assert.Equal(obj, obj2);
            Assert.NotEqual(obj, obj3); // they are not equal because an open generic type cannot be redirected to implementation type.
        }
    }

    [Fact]
    public void GenericTransient_Test()
    {
        Test<int>();
        Test<string>();

        void Test<T>()
        {
            var obj = ServiceProvider.GetRequiredService<IGenericTransient<T>>();
            var obj2 = ServiceProvider.GetRequiredService<IGenericTransient<T>>();
            var obj3 = ServiceProvider.GetRequiredService<GenericTransient<T>>();
            Assert.NotEqual(obj, obj2);
            Assert.NotEqual(obj, obj3);
            Assert.NotEqual(obj2, obj3);
        }
    }

    [Fact]
    public void NonGenericTransient_Test()
    {
        var obj = ServiceProvider.GetService<INonGenericTransient>();
        Assert.Null(obj);

        var obj2 = ServiceProvider.GetService<NonGenericTransient<int>>();
        Assert.NotNull(obj2);
    }
}