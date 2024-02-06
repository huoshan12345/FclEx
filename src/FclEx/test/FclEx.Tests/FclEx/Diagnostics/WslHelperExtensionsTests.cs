namespace FclEx.Diagnostics;

public class WslHelperExtensionsTests
{
    [Fact]
    public async Task WslPath_Test()
    {
        var path = await ProcessHelper.Wsl.WslPath(@"D:\projects\FclEx\.github\workflows");
        Assert.Equal("/mnt/d/projects/FclEx/.github/workflows", path);
    }
}