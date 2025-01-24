namespace FclEx.Utils.Diagnostics;

public class PwshRunnerTests
{
    [Fact]
    public async Task GetChildItem_Test()
    {
        var result = await PwshRunner.Instance.ExecuteAsync(new ProcessCommand("Get-ChildItem", AppContext.BaseDirectory));
        Assert.Contains("Directory:", result);
    }
}
