namespace FclEx.Utils.Diagnostics;

public class ProcessRunnerTests
{
    [LocalOnlyFact]
    public async Task GetChildItem_Test()
    {
        using var runner = new ProcessRunner("pwsh");
        var result = await runner.ExecuteAsync("Get-Location");
        Assert.Contains("Directory:", result);

        await runner.ExecuteAsync("exit");
        await runner.Process.WaitForExitAsync();
    }
}
