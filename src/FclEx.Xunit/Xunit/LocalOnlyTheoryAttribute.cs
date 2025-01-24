namespace Xunit;

public class LocalOnlyTheoryAttribute : TheoryAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];

    public BuildTypeOption RequiredBuildType { get; set; } = BuildTypeOption.Debug;

    private string? _skip;
    public override string? Skip
    {
        get => TestHelper.GetSkipReason(new(_skip, RequiredBuildType, AllowedOSPlatforms, TestHelper.GithubActionEnvKey, null, EnvVarCheckOption.NotExist));
        set => _skip = value;
    }
}