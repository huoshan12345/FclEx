namespace FclEx.Utils.Diagnostics;

public class PwshInvokerTests
{
    [Fact]
    public async Task GetChildItem_Test()
    {
        var result = await PwshInvoker.Instance.ExecuteAsync(new ProcessInvocation("Get-ChildItem", AppContext.BaseDirectory));
        Assert.Contains("Directory:", result);
    }
}
