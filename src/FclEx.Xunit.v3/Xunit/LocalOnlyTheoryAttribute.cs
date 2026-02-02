using FclEx.Xunit;

namespace Xunit;

public class LocalOnlyTheoryAttribute : TheoryAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];
    public BuildTypeOption RequiredBuildType { get; set; } = BuildTypeOption.Debug;

    public LocalOnlyTheoryAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        Skip = TestHelper.GetSkipReason(new(null, RequiredBuildType, AllowedOSPlatforms, TestHelper.GithubActionEnvKey, null, EnvVarCheckOption.NotExist));
    }
}