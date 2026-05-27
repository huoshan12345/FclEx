namespace FclEx.Extensions;

public class EnvironmentExtensionsTests
{
    [Fact]
    public void IsUnderTest_ReturnsTrueWhenRunningInTestHost()
    {
        Assert.True(Environment.IsUnderTest);
    }

    [Fact]
    public void IsWindows_ReturnsRuntimeInformationResult()
    {
        Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), Environment.IsWindows);
    }

    [Fact]
    public void IsLinux_ReturnsRuntimeInformationResult()
    {
        Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Linux), Environment.IsLinux);
    }

    [Fact]
    public void IsMacOS_ReturnsRuntimeInformationResult()
    {
        Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.OSX), Environment.IsMacOS);
    }

    [Fact]
    public void IsFreeBSD_ReturnsRuntimeInformationResult()
    {
        Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD), Environment.IsFreeBSD);
    }
}
