using static FclEx.Xunit.EnvVarCheckOption;

namespace FclEx.Xunit;

public record SkipReasonInfo(
    string? Reason,
    BuildTypeOption RequiredBuildType,
    OSPlatformType[]? AllowedOSPlatforms,
    string? EnvVarKey,
    string? EnvVarValue,
    EnvVarCheckOption EnvVarCheckOption);

public enum BuildTypeOption
{
    Any,
    Debug,
    Release,
}

public enum EnvVarCheckOption
{
    None,
    Equal,
    NotEqual,
    Exist,
    NotExist,
}

public static class TestHelper
{
    /// <summary>
    /// The environment variable key for retrieving the current GitHub Action name or step ID.
    /// </summary>
    /// <remarks>
    /// This constant represents the key "GITHUB_ACTION", which can be used with the 
    /// <see cref="Environment.GetEnvironmentVariable(string)"/> method to obtain
    /// the name of the action currently running, or the id of a step.<br/>
    /// For example, for an action, __repo-owner_name-of-action-repo.
    /// </remarks>
    public const string GithubActionEnvKey = "GITHUB_ACTION";

    /// <summary>
    /// Indicates whether the current environment is running within a GitHub Action context.
    /// </summary>
    /// <remarks>
    /// This static readonly field evaluates to <see langword="true"/> if the environment variable associated
    /// with <see cref="GithubActionEnvKey"/> ("GITHUB_ACTION") is set and contains a non-empty value.<br/>
    /// This allows for conditionally executing logic based 
    /// on whether the code is being run as part of a GitHub Action workflow.
    /// </remarks>
    public static readonly bool IsGithubAction = Environment.GetEnvironmentVariable(GithubActionEnvKey).IsNullOrEmpty() == false;
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
    public static readonly string AssemblyFullName = typeof(TestHelper).Assembly.GetName().FullName;

    public static readonly Assembly[] ReferencingAssemblies = Assemblies
        .Where(m => m.GetReferencedAssemblies().Any(x => x.FullName == AssemblyFullName))
        .ToArray();

    public static readonly bool[] ReferencingAssembliesJitOptimized = ReferencingAssemblies
        .Select(m => m.IsJitOptimized())
        .Distinct()
        .ToArray();

    public static bool IsRunningUnderReSharper()
    {
        return Assemblies.Any(a => a.FullName?.StartsWith("ReSharperTestRunner", StringComparison.OrdinalIgnoreCase) == true);
    }

    public static string? GetSkipReason(SkipReasonInfo info)
    {
        if (info.Reason is { Length: > 0 } reason)
        {
            return reason;
        }

        if (info.AllowedOSPlatforms is { } os && os.Any(m => RuntimeInformation.IsOSPlatform(m.ToOSPlatform()) == false))
        {
            return $"The current operating system is not any of {os.JoinWith(", ")}";
        }

        if (info.RequiredBuildType is BuildTypeOption.Debug or BuildTypeOption.Release)
        {
            var currentBuildType = ReferencingAssembliesJitOptimized.Any(m => m == false)
                ? BuildTypeOption.Debug
                : BuildTypeOption.Release;

            if (currentBuildType != info.RequiredBuildType)
            {
                return $"The calling assembly is not in {currentBuildType} mode";
            }
        }

        // ReSharper disable once InvertIf
        if (info is { EnvVarKey: { Length: > 0 } key, EnvVarValue: var value, EnvVarCheckOption: var option } && option != None)
        {
            var actualValue = Environment.GetEnvironmentVariable(key);
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (option)
            {
                case Exist when actualValue == null:
                    return $"The environment variable {key} does not exist";
                case NotExist when actualValue != null:
                    return $"The environment variable {key} exist";
                case Equal when actualValue != value:
                    return $"The environment variable {key}'s actual value is {actualValue}, not {value}";
                case NotEqual when actualValue == value:
                    return $"The environment variable {key}'s actual value equals to {value}";
            }
        }

        return null;
    }
}