#pragma warning disable IDE0001

namespace Microsoft.AspNetCore.Mvc.Testing;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected static readonly string ApplicationName = typeof(TStartup).Assembly.GetName().Name!;

    protected virtual string EnvironmentName => "Testing";

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseApplicationName(ApplicationName)
            .UseEnvironment(EnvironmentName)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureWebHost(m => m.UseStartup(CreateStartup)
                .UseApplicationName(ApplicationName) // Set the application name, otherwise all web apis will return 404
                .UseTestContentRoot<TStartup>(AppContext.BaseDirectory));
    }

    protected virtual TStartup CreateStartup(WebHostBuilderContext context)
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