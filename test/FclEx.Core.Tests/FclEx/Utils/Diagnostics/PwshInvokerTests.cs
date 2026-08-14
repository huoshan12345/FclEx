namespace FclEx.Utils.Diagnostics;

public class PwshInvokerTests
{
    [Fact]
    public async Task GetChildItem_Test()
    {
        var result = await PwshInvoker.Instance.ExecuteAsync(new ProcessInvocation("Get-ChildItem", AppContext.BaseDirectory));
        Assert.Contains("Directory:", result);
    }

    [Fact]
    public async Task ExecuteAsync_CapturesTrailingStandardOutputAndError()
    {
        const string command = "1..2000 | ForEach-Object { [Console]::Out.WriteLine(('stdout-' + $_)); [Console]::Error.WriteLine(('stderr-' + $_)) }; [Console]::Out.WriteLine('stdout-tail'); [Console]::Error.WriteLine('stderr-tail')";

        var result = await PwshInvoker.Instance.ExecuteAsync(command);

        var lines = result.Split([Environment.NewLine], StringSplitOptions.None);
        Assert.Contains("stdout-tail", lines);
        Assert.Contains("stderr-tail", lines);
    }

    [LocalOnlyFact]
    public async Task ExecuteAsync_Cancellation_TerminatesProcess()
    {
        var processIdFile = Path.Combine(Path.GetTempPath(), $"fclex-process-{Guid.NewGuid():N}.pid");
        var completedFile = Path.Combine(Path.GetTempPath(), $"fclex-process-{Guid.NewGuid():N}.completed");
        using var cancellation = new CancellationTokenSource();
        Task<string>? execution = null;
        int? processId = null;

        try
        {
            var command = $"Set-Content -LiteralPath '{processIdFile}' -Value $PID; Start-Sleep -Seconds 30; Set-Content -LiteralPath '{completedFile}' -Value completed";
            execution = PwshInvoker.Instance.ExecuteAsync(new ProcessInvocation(command, CancellationToken: cancellation.Token));

            var parsedProcessId = 0;
            var started = SpinWait.SpinUntil(() => TryReadProcessId(processIdFile, out parsedProcessId), TimeSpan.FromSeconds(10));
            Assert.True(started, "The child process did not write its process ID within the timeout.");
            processId = parsedProcessId;

            await cancellation.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => execution);
            Assert.False(IsProcessRunning(processId.Value));
            Assert.False(File.Exists(completedFile));
        }
        finally
        {
            await cancellation.CancelAsync();

            if (execution is not null)
            {
                try
                {
                    await execution;
                }
                catch
                {
                    // The test intentionally cancels the invocation.
                }
            }

            if (processId.HasValue)
                TryKill(processId.Value);

            File.Delete(processIdFile);
            File.Delete(completedFile);
        }
    }

    private static bool IsProcessRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool TryReadProcessId(string path, out int processId)
    {
        try
        {
            return int.TryParse(File.ReadAllText(path).Trim(), out processId);
        }
        catch (IOException)
        {
            processId = 0;
            return false;
        }
    }

    private static void TryKill(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            if (!process.HasExited)
                process.Kill();
        }
        catch (ArgumentException)
        {
            // The process has already exited.
        }
        catch (InvalidOperationException)
        {
            // The process exited while the test was cleaning up.
        }
    }
}
