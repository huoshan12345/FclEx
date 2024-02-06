using FclEx.Abp;

namespace Microsoft.Extensions.Hosting;

public class ExtensionsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UseLightInject_Test(bool useAop)
    {
        var builder = new HostBuilder()
            .UseLightInject(useAop)
            .ConfigureServices((context, services) => services.AddApplication<AbpTestModule>());

        using var host = builder.Build();
        host.Services.UseAbp();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        await host.RunAsync(cts.Token);
    }
}