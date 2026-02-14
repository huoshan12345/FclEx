namespace Microsoft.AspNetCore.Hosting;

public static class HostBuilderExtensions
{
    /// <summary>
    /// Sets the test content root environment variable for <typeparamref name="TStartup"/>.
    /// </summary>
    /// <typeparam name="TStartup">
    /// The startup type whose assembly name is used to build the setting key.
    /// </typeparam>
    /// <param name="builder">The <see cref="IWebHostBuilder"/>.</param>
    /// <returns>The same <see cref="IWebHostBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Enables WebApplicationFactory to resolve the content root from
    /// "TEST_CONTENTROOT_{ASSEMBLY_NAME}" via the corresponding
    /// "ASPNETCORE_TEST_CONTENTROOT_{ASSEMBLY_NAME}" environment variable.
    /// </remarks>
    public static IWebHostBuilder SetWebApplicationFactoryContentRoot<TStartup>(this IWebHostBuilder builder)
    {
        var assemblyName = typeof(TStartup).Assembly.GetName().Name;
        var settingSuffix = assemblyName?.ToUpperInvariant().Replace(".", "_");
        var settingName = $"ASPNETCORE_TEST_CONTENTROOT_{settingSuffix}";
        Environment.SetEnvironmentVariable(settingName, AppContext.BaseDirectory);
        return builder;
    }
}