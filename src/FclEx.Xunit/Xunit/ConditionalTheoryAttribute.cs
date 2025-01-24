namespace Xunit;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class ConditionalTheoryAttribute : TheoryAttribute
{
    public OSPlatformType[]? AllowedOSPlatforms { get; set; }
    public BuildTypeOption BuildType { get; set; }
    public string? EnvVarKey { get; set; }
    public string? EnvVarValue { get; set; }
    public EnvVarCheckOption EnvVarCheckOption { get; set; }

    private string? _skip;
    public override string? Skip
    {
        get => TestHelper.GetSkipReason(new(_skip, BuildType, AllowedOSPlatforms, EnvVarKey, EnvVarValue, EnvVarCheckOption));
        set => _skip = value;
    }
}