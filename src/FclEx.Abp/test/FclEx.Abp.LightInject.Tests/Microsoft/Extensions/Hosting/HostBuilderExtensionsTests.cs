using FclEx.Abp;

namespace Microsoft.Extensions.Hosting;

public class HostBuilderExtensionsTests
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
        await host.Services.UseAbpAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.2));
        await host.RunAsync(cts.Token);
    }
}