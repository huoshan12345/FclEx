namespace FclEx.Xunit;

public static class LocalTestHelper
{
    public static readonly bool IsGithubAction = Environment.GetEnvironmentVariable("GITHUB_ACTION").IsNullOrEmpty() == false;

    public static string? GetSkipReason(string? reason, OSPlatformType? localOS, Assembly? assemblyToCheckDebug)
    {
        if (reason is { Length: > 0 })
        {
            return reason;
        }

        if (IsGithubAction)
        {
            return "Github action is being used";
        }

        if (localOS is { } os && RuntimeInformation.IsOSPlatform(os.ToOSPlatform()) == false)
        {
            return $"The current operating system is not {os}";
        }

        if (assemblyToCheckDebug != null && assemblyToCheckDebug.IsDebug() == false)
        {
            return $"The executing assembly {assemblyToCheckDebug.GetName().Name} is not in debug mode";
        }

        return null;
    }
}