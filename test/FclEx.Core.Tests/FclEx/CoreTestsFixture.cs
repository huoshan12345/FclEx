namespace FclEx;

public class CoreTestsFixture : IAsyncLifetime
{
    public static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
    public static readonly IConfigurationRoot Config = BuildConfig();

    public Assembly CurrentAssembly { get; }

    public CoreTestsFixture()
    {
        CurrentAssembly = GetType().Assembly;
    }

    public static IConfigurationRoot BuildConfig()
    {
        var env = TestHelper.IsGithubAction ? "github" : "local";

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, false)
            .AddJsonFile("appsettings.decrypted.json", true, false)
            .AddJsonFile($"appsettings.{env}.json", true, false);

        return builder.Build();
    }

    public virtual ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

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
    public static string? WithAssemblyInfo(string? str, string? assemblyName, int dotNetVersion, string os, char separator = '_')
    {
        if (str is null)
            return null;

        // used to ensure every test assembly uses unique service, such as database.
        return StringBuilderHelper.Build(m =>
        {
            if (str.IsNotEmpty())
            {
                m.Append(str);
                m.Append(separator);
            }

            if (assemblyName.IsNotEmpty())
            {
                m.Append(assemblyName);
                m.Append(separator);
            }

            m.Append(dotNetVersion);
            m.Append(separator);
            m.Append(os);
        });
    }


    [return: NotNullIfNotNull(nameof(str))]
    public static string? WithAssemblyInfo(string? str, Assembly assembly, char separator = '_')
    {
        if (str is null)
            return null;

        var assemblyShortName = assembly.GetName().Name.TrimStart("FclEx.").TrimEnd(".Tests")?.ToLower();
        var os = GetOSName().ToLower();
        return WithAssemblyInfo(str, assemblyShortName, Environment.Version.Major, os, separator);
    }

    [return: NotNullIfNotNull(nameof(str))]
    public string? WithAssemblyInfo(string? str, char separator = '_')
    {
        return WithAssemblyInfo(str, CurrentAssembly, separator);
    }

    public static string GetOSName()
    {
        var os = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? nameof(OSPlatform.Windows)
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? nameof(OSPlatform.Linux)
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? nameof(OSPlatform.OSX)
                    : RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                        ? "FreeBSD"
                        : "Unknown";
        return os;
    }
}
