using FclEx.Xunit;

namespace FclEx.Tests;

public class GlobalFixture : IAsyncLifetime
{
    public GlobalFixture()
    {
        CurrentAssembly = GetType().Assembly;
    }

    public static IConfigurationRoot BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .AddJsonFile("appsettings.decrypted.json", true, false);

        return builder.Build();
    }

    public static IConfigurationRoot Config { get; } = BuildConfig();

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public Assembly CurrentAssembly { get; }

#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        var count = Environment.ProcessorCount;
        ThreadPool.SetMinThreads(count * 4, count * 2);
#pragma warning disable SYSLIB0014
        ServicePointManager.DefaultConnectionLimit = short.MaxValue;
#pragma warning restore SYSLIB0014
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [return: NotNullIfNotNull(nameof(str))]
    public string? WithAssemblyInfoIfNotNull(string? str, char separator = '_')
    {
        return str is null
            ? null
            : WithAssemblyInfo(str, separator);
    }

    public string WithAssemblyInfo(string str, char separator = '_')
    {
        // used to ensure every test assembly uses unique service, such as database.
        return StringBuilderHelper.Build(m =>
        {
            if (str.IsNotEmpty())
            {
                m.Append(str);
                m.Append(separator);
            }

            var assemblyName = CurrentAssembly.GetName().Name;
            if (assemblyName.IsNotEmpty())
            {
                // as short as possible cause database don't like long name.
                var name = assemblyName.TrimStart("FclEx").TrimEnd("Tests").Replace(".", "").ToLower();
                if (name.IsNotEmpty())
                {
                    m.Append(name);
                    m.Append(separator);
                }
            }

            m.Append(Environment.Version.Major);
        });
    }
}
