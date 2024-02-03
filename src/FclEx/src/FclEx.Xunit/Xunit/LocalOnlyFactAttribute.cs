namespace Xunit;

public class LocalOnlyFactAttribute : FactAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];

    private string? _skip;
    public override string? Skip
    {
        get => TestHelper.GetSkipReason(new(_skip, BuildTypeOption.Debug, AllowedOSPlatforms, TestHelper.EnvKeyOfGithubAction, null, EnvVarCheckOption.NotExist));
        set => _skip = value;
    }
}