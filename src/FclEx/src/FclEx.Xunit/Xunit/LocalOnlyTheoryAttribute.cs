namespace Xunit;

public class LocalOnlyTheoryAttribute : TheoryAttribute
{
    public OSPlatformType? LocalOS { get; set; } = OSPlatformType.Windows;
    public Type? TypeToCheckDebug { get; set; }

    private string? _skip;
    public override string? Skip
    {
        get => LocalTestHelper.GetSkipReason(_skip, LocalOS, TypeToCheckDebug?.Assembly);
        set => _skip = value;
    }
}