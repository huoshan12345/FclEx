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
            .AddEnvironmentVariables("FclEx_");

        if (TestHelper.IsGithubAction)
        {
            builder.AddJsonFile("appsettings.github.json", true, false);
        }
        else
        {
            var machineName = Environment.MachineName.ToLower();
            builder.AddJsonFile($"appsettings.{machineName}.json", true, false);
        }

        return builder.Build();
    }

    public static IConfigurationRoot Config { get; } = BuildConfig();

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public virtual Task DisposeAsync() => Task.CompletedTask;

    public Assembly CurrentAssembly { get; }

    [ModuleInitializer]
    public static void Initialize()
    {
        ThreadPool.SetMinThreads(100, 100);
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
