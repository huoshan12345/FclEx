namespace Microsoft.Extensions.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    public class TestHostService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public class TestHostService2 : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    [Fact]
    public void AddHostedService_Test()
    {
        var services = new ServiceCollection()
            .AddHostedService(typeof(TestHostService))
            .AddHostedService(typeof(TestHostService2));

        var expectedService = new ServiceCollection()
            .AddHostedService<TestHostService>()
            .AddHostedService<TestHostService2>();

        Assert.Equal(expectedService, services, ServiceDescriptorEqualityComparer.Instance);
    }

    [Fact]
    public void AddHostServicesInNamespace_Test()
    {
        var t = GetType();
        var services = new ServiceCollection().AddHostedService(t.Assembly);

        var expectedService = new ServiceCollection()
            .AddHostedService<TestHostService>()
            .AddHostedService<TestHostService2>();

        Assert.Equal(expectedService, services, ServiceDescriptorEqualityComparer.Instance);
    }

    [Fact]
    public void AddSameType_Test()
    {
        var provider = new ServiceCollection()
            .AddSingleton<IHostedService, TestHostService>()
            .AddSingleton<IHostedService, TestHostService>()
            .BuildServiceProvider();

        Assert.Equal(2, provider.GetServices<IHostedService>().Count());
    }
}