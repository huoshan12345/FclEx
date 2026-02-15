using Microsoft.AspNetCore.Mvc.Testing;

namespace Microsoft.AspNetCore.Hosting;

public static class WebHostBuilderExtensions
{
    /// <summary>
    /// Sets the environment variable used by <see cref="WebApplicationFactory{T}"/> to override the application's
    /// ContentRoot during testing.
    /// </summary>
    /// <remarks>
    /// The variable name is constructed as: ASPNETCORE_TEST_CONTENTROOT_{ASSEMBLY_NAME}.<br/>
    /// <br/>
    /// <see cref="WebHostBuilder"/> loads environment variables with the "ASPNETCORE_" prefix.<br/>
    /// This makes the value available as the setting: TEST_CONTENTROOT_{ASSEMBLY_NAME}.<br/>
    /// <br/>
    /// <see cref="WebApplicationFactory{T}"/>.<see cref="SetContentRootFromSetting"/> reads that setting via <br/>
    /// <see cref="IWebHostBuilder"/>.<see cref="IWebHostBuilder.GetSetting"/> and, if present, calls UseContentRoot with the value.
    /// </remarks>
    public static IWebHostBuilder UseTestContentRoot<TStartup>(this IWebHostBuilder builder, string testContentRoot)
    {
        var assemblyName = typeof(TStartup).Assembly.GetName().Name;
        var settingSuffix = assemblyName?.ToUpperInvariant().Replace(".", "_");
        var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{settingSuffix}";
        Environment.SetEnvironmentVariable(settingName, testContentRoot);
        return builder;
    }
}