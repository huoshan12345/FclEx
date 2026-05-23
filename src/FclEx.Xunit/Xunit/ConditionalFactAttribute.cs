namespace Xunit;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ConditionalFactAttribute : FactAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; }
    public BuildTypeOption BuildType { get; set; }
    public string? EnvVarKey { get; set; }
    public string? EnvVarValue { get; set; }
    public EnvVarCheckOption EnvVarCheckOption { get; set; }

#if FCLEX_XUNIT_V3
    public ConditionalFactAttribute(
         [CallerFilePath] string? sourceFilePath = null,
         [CallerLineNumber] int sourceLineNumber = -1)
         : base(sourceFilePath, sourceLineNumber)
#else
    public ConditionalFactAttribute()
#endif
    {
        Skip = TestHelper.GetSkipReason(new(null, BuildType, AllowedOSPlatforms, EnvVarKey, EnvVarValue, EnvVarCheckOption));
    }
}