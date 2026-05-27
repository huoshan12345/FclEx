namespace FclEx.Extensions;

public static class EnvironmentExtensions
{
    private static readonly string[] TestAssemblyNamePrefixes =
    [
        "xunit",
        "nunit",
        "Microsoft.VisualStudio.TestPlatform",
        "Microsoft.TestPlatform",
        "MSTest",
        "testhost",
    ];

    private static bool _isUnderTestDetected;
    private static int _lastNonTestAssemblyCount = -1;

    private static bool DetectIsUnderTest(Assembly[] assemblies)
    {
        return assemblies.Any(a =>
            a.GetName().Name is { } name &&
            TestAssemblyNamePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
    }

    extension(Environment)
    {
        /// <summary>
        /// Gets whether the current application domain appears to be running under a common test framework or test host.
        /// </summary>
        /// <remarks>
        /// This is a best-effort heuristic based on loaded assemblies, so custom runners or unusual load timing may produce false negatives.
        /// A positive result is cached; a negative result is reused only while the loaded assembly count is unchanged.
        /// If assemblies are unloaded or replaced without changing the assembly count, the result may be stale.
        /// </remarks>
        public static bool IsUnderTest
        {
            get
            {
                if (_isUnderTestDetected)
                    return true;

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies.Length == _lastNonTestAssemblyCount)
                    return false;

                if (DetectIsUnderTest(assemblies))
                {
                    _isUnderTestDetected = true;
                    return true;
                }

                _lastNonTestAssemblyCount = assemblies.Length;
                return false;
            }
        }

        /// <summary>
        /// Gets whether the current process is running on Windows.
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Gets whether the current process is running on Linux.
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// Gets whether the current process is running on macOS.
        /// </summary>
        public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Gets whether the current process is running on FreeBSD.
        /// </summary>
        public static bool IsFreeBSD => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
    }
}
