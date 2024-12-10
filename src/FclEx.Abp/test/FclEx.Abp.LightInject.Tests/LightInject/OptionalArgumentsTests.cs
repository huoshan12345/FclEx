namespace LightInject;

public class OptionalArgumentsTests
{
    public class A { }

    public class B
    {
        private readonly A? _a;

        public B(A? a = null)
        {
            _a = a;
        }

        public bool HasA() => _a != null;
    }

    private static IServiceProvider Create(bool withA)
    {
        var services = new ServiceCollection()
            .AddTransient<B>();
        if (withA)
        {
            services.AddTransient<A>();
        }

        var container = LightInjectHelper.CreateContainer()
            .RegisterMsDiService(services);

        var provider = container.GetInstance<IServiceProvider>();
        return provider;
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Test(bool withA)
    {
        var provider = Create(withA);
        var b = provider.GetRequiredService<B>();
        Assert.Equal(withA, b.HasA());
    }
}