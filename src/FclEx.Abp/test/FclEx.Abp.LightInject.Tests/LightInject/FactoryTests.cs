namespace LightInject;

public class FactoryTests
{
    public class Tester : IDisposable
    {
        public static int InstanceCount { get; private set; }
        public int DisposedCount { get; private set; }

        public Tester()
        {
            InstanceCount++;
        }

        public void Dispose()
        {
            DisposedCount++;
        }
    }

    private static IServiceProvider Create()
    {
        var services = new ServiceCollection()
            .AddSingleton(m => new Tester());

        var container = LightInjectHelper.CreateContainer();
        container.RegisterMsDiService(services);

        var provider = container.GetInstance<IServiceProvider>();
        return provider;
    }

    [Fact]
    public void Dispose_Test()
    {
        var provider = Create();
        var obj = provider.GetRequiredService<Tester>();
        Assert.Equal(1, Tester.InstanceCount);
        Assert.Equal(0, obj.DisposedCount);
        provider.ToDisposable().Dispose();
        Assert.Equal(1, obj.DisposedCount);
    }
}