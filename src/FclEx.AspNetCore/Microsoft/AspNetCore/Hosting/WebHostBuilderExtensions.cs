namespace Microsoft.AspNetCore.Hosting;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseApplicationName(this IWebHostBuilder builder, string applicationName)
    {
        var settingName = $"ASPNETCORE_{WebHostDefaults.ApplicationKey}";
        Environment.SetEnvironmentVariable(settingName, applicationName);
        return builder;
    }

    public static IWebHostBuilder UseApplicationName<TStartup>(this IWebHostBuilder builder)
    {
        var assemblyName = typeof(TStartup).Assembly.GetName().Name;
        return builder.UseApplicationName(assemblyName!);
    }
}