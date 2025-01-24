namespace FclEx.Utils;

public class WslHelperExtensionsTests
{
    [LocalOnlyFact]
    public async Task WslPath_Test()
    {
        var path = await ProcessHelper.Wsl.WslPath(@"D:\projects\FclEx\.github\workflows");
        Assert.Equal("/mnt/d/projects/FclEx/.github/workflows", path);
    }
}