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

public static class BuildTypeOptionExtensions
{
    public static BuildType? ToBuildType(this BuildTypeOption option)
    {
        return option switch
        {
            BuildTypeOption.Debug => BuildType.Debug,
            BuildTypeOption.Release => BuildType.Release,
            BuildTypeOption.Any => null,
            _ => throw new ArgumentOutOfRangeException(nameof(option), option, null)
        };
    }
}


public static class TestHelper
{
    public const string EnvKeyOfGithubAction = "GITHUB_ACTION";
    public static readonly bool IsGithubAction = Environment.GetEnvironmentVariable(EnvKeyOfGithubAction).IsNullOrEmpty() == false;

    public static readonly Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
    public static readonly string AssemblyFullName = typeof(TestHelper).Assembly.GetName().FullName;
    public static readonly Assembly[] ReferencingAssemblies = Assemblies
        .Where(m => m.GetReferencedAssemblies().Any(x => x.FullName == AssemblyFullName))
        .ToArray();

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

        if (info.RequiredBuildType.ToBuildType() is { } buildType)
        {
            if (ReferencingAssemblies is [var assembly])
            {
                if (assembly.GetBuildInfo().BuildType != buildType)
                {
                    return $"The calling assembly is not in {buildType} mode";
                }
            }
            else
            {
                return $"The count of referencing assemblies is {ReferencingAssemblies.Length}, not 1.";
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