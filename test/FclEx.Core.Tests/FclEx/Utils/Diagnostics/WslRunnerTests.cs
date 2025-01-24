namespace FclEx.Utils.Diagnostics;

public class WslRunnerTests
{
    [LocalOnlyFact]
    public async Task WslPath_Test()
    {
        var path = await WslRunner.Instance.WslPath(@"D:\projects\FclEx\.github\workflows");
        Assert.Equal("/mnt/d/projects/FclEx/.github/workflows", path);
    }
}