namespace Volo.Abp.DependencyInjection;

public class OpenGenericConventionalRegistrarTests(AbpTestsFixture fixture) : AbpTests(fixture)
{
    public interface IGenericSingleton<out T> : ISingletonDependency;
    public class GenericSingleton<T> : IGenericSingleton<T>;

    public interface IGenericTransient<T> : ITransientDependency;
    public class GenericTransient<T> : IGenericTransient<T>;

    public interface INonGenericTransient : ITransientDependency;
    public class NonGenericTransient<T> : INonGenericTransient;

    [Fact]
    public void GenericSingleton_Test()
    {
        Test<int>();
        Test<string>();

        void Test<T>()
        {
            var obj = Services.GetRequiredService<IGenericSingleton<T>>();
            var obj2 = Services.GetRequiredService<IGenericSingleton<T>>();
            var obj3 = Services.GetRequiredService<GenericSingleton<T>>();
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
            var obj = Services.GetRequiredService<IGenericTransient<T>>();
            var obj2 = Services.GetRequiredService<IGenericTransient<T>>();
            var obj3 = Services.GetRequiredService<GenericTransient<T>>();
            Assert.NotEqual(obj, obj2);
            Assert.NotEqual(obj, obj3);
            Assert.NotEqual(obj2, obj3);
        }
    }

    [Fact]
    public void NonGenericTransient_Test()
    {
        var obj = Services.GetService<INonGenericTransient>();
        Assert.Null(obj);

        var obj2 = Services.GetService<NonGenericTransient<int>>();
        Assert.NotNull(obj2);
    }
}