namespace Xunit;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ConditionalFactAttribute : FactAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; }
    public BuildTypeOption BuildType { get; set; }
    public string? EnvVarKey { get; set; }
    public string? EnvVarValue { get; set; }
    public EnvVarCheckOption EnvVarCheckOption { get; set; }

    public ConditionalFactAttribute(
         [CallerFilePath] string? sourceFilePath = null,
         [CallerLineNumber] int sourceLineNumber = -1)
         : base(sourceFilePath, sourceLineNumber)
    {
        Skip = TestHelper.GetSkipReason(new(null, BuildType, AllowedOSPlatforms, EnvVarKey, EnvVarValue, EnvVarCheckOption));
    }
}