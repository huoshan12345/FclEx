namespace Xunit;

public class LocalOnlyFactAttribute : FactAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; } = [OSPlatformType.Windows];

#if FCLEX_XUNIT_V3
    public LocalOnlyFactAttribute(
         [CallerFilePath] string? sourceFilePath = null,
         [CallerLineNumber] int sourceLineNumber = -1)
         : base(sourceFilePath, sourceLineNumber)
#else
    public LocalOnlyFactAttribute()
#endif
    {
        Skip = TestHelper.GetSkipReason(new(null, BuildTypeOption.Debug, AllowedOSPlatforms, TestHelper.GithubActionEnvKey, null, EnvVarCheckOption.NotExist));
    }
}