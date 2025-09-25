namespace FclEx.Utils.Diagnostics;

public class WslInvokerTests
{
    [LocalOnlyFact]
    public async Task WslPath_Test()
    {
        var path = await WslInvoker.Instance.WslPath(@"D:\projects\FclEx\.github\workflows");
        Assert.Equal("/mnt/d/projects/FclEx/.github/workflows", path);
    }
}