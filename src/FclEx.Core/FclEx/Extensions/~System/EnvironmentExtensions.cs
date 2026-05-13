namespace FclEx.Extensions;

public static class EnvironmentExtensions
{
    private static readonly string[] TestAssemblyPrefixes =
    [
        "xunit",
        "nunit",
        "Microsoft.VisualStudio.TestPlatform",
        "Microsoft.TestPlatform",
        "MSTest",
        "testhost",
    ];

    private static readonly Lazy<bool> _isUnderTest = new(()
        => AppDomain.CurrentDomain.GetAssemblies().Any(a =>
            a.FullName is { } name && TestAssemblyPrefixes.Any(name.StartsWith)));

    extension(Environment)
    {
        public static bool IsUnderTest => _isUnderTest.Value;
    }
}
