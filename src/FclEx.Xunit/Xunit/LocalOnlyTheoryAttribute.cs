namespace Xunit;

public class LocalOnlyTheoryAttribute : TheoryAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];

#if FCLEX_XUNIT_V3
    public LocalOnlyTheoryAttribute(
         [CallerFilePath] string? sourceFilePath = null,
         [CallerLineNumber] int sourceLineNumber = -1)
         : base(sourceFilePath, sourceLineNumber)
#else
    public LocalOnlyTheoryAttribute()
#endif
    {
        Skip = TestHelper.GetSkipReason(new(null, BuildTypeOption.Debug, AllowedOSPlatforms, TestHelper.GithubActionEnvKey, null, EnvVarCheckOption.NotExist));
    }
}