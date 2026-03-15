using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseApplicationName(this IHostBuilder builder, string applicationName)
    {
        return builder.ConfigureHostConfiguration(configBuilder =>
        {
            configBuilder.AddInMemoryCollection([KeyValuePair.Create(HostDefaults.ApplicationKey, applicationName)!]);
        });
    }

    public static IHostBuilder UseApplicationName<TStartup>(this IHostBuilder builder)
    {
        var assemblyName = typeof(TStartup).Assembly.GetName().Name;
        return builder.UseApplicationName(assemblyName!);
    }
}
