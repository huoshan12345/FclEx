#pragma warning disable IDE0001

namespace Microsoft.AspNetCore.Mvc.Testing;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected virtual string EnvironmentName => "Testing";

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseEnvironment(EnvironmentName)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureWebHost(m => m.UseStartup(Create).SetWebApplicationFactoryContentRoot<TStartup>());
    }

    private static TStartup Create(WebHostBuilderContext context)
    {
        var env = context.HostingEnvironment;
        return new ServiceCollection()
#pragma warning disable CS0618 // Type or member is obsolete
            .AddSingleton(typeof(Microsoft.Extensions.Hosting.IHostingEnvironment), env)
            .AddSingleton(typeof(Microsoft.AspNetCore.Hosting.IHostingEnvironment), env)
#pragma warning restore CS0618 // Type or member is obsolete
            .AddSingleton(env)
            .AddSingleton(context.Configuration)
            .AddSingleton<TStartup>()
            .BuildServiceProvider()
            .GetRequiredService<TStartup>();
    }
}