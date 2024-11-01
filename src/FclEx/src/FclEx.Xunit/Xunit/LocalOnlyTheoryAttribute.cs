namespace Xunit;

public class LocalOnlyTheoryAttribute : TheoryAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];

    private string? _skip;
    public override string? Skip
    {
        get => TestHelper.GetSkipReason(new(_skip, BuildTypeOption.Debug, AllowedOSPlatforms, TestHelper.GithubActionEnvKey, null, EnvVarCheckOption.NotExist));
        set => _skip = value;
    }
}