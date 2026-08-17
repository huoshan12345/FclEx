namespace FclEx.Utils;

public class PwshInvokerTests
{
    [Fact]
    public async Task GetChildItem_Test()
    {
        var result = await PwshInvoker.Instance.ExecuteAsync(new ProcessInvocation("Get-ChildItem", AppContext.BaseDirectory));
        Assert.Contains("Directory:", result.StandardOutput);
    }

    [Fact]
    public async Task ExecuteAsync_CapturesTrailingStandardOutputAndError()
    {
        const string command = "1..2000 | ForEach-Object { [Console]::Out.WriteLine(('stdout-' + $_)); [Console]::Error.WriteLine(('stderr-' + $_)) }; [Console]::Out.WriteLine('stdout-tail'); [Console]::Error.WriteLine('stderr-tail')";

        var result = await PwshInvoker.Instance.ExecuteAsync(command);

        var outputLines = result.StandardOutput.Split([Environment.NewLine], StringSplitOptions.None);
        var errorLines = result.StandardError.Split([Environment.NewLine], StringSplitOptions.None);
        Assert.Contains("stdout-tail", outputLines);
        Assert.Contains("stderr-tail", errorLines);
    }

    [Fact]
    public async Task ExecuteAsync_PreservesQuotedAndMultilineCommand()
    {
        const string command = "Write-Output 'a \"quoted\" value'\nWrite-Output 'second line'";

        var result = await PwshInvoker.Instance.ExecuteAsync(command);

        Assert.Equal($"a \"quoted\" value{Environment.NewLine}second line", result.StandardOutput);
        Assert.Empty(result.StandardError);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnResultPolicy_PreservesExitCodeAndSeparateStreams()
    {
        var invocation = new ProcessInvocation(
            "[Console]::Out.WriteLine('output'); [Console]::Error.WriteLine('error'); exit 7",
            ExitCodePolicy: ProcessExitCodePolicy.ReturnResult);

        var result = await PwshInvoker.Instance.ExecuteAsync(invocation);

        Assert.Equal(7, result.ExitCode);
        Assert.False(result.Succeeded);
        Assert.Equal("output", result.StandardOutput);
        Assert.Equal("error", result.StandardError);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowPolicy_IncludesStructuredResult()
    {
        var exception = await Assert.ThrowsAsync<ProcessException>(() =>
            PwshInvoker.Instance.ExecuteAsync("[Console]::Error.WriteLine('failure'); exit 9"));

        Assert.Equal(9, exception.ExitCode);
        Assert.Equal("failure", exception.Result.StandardError);
        Assert.Empty(exception.Result.StandardOutput);
    }

    [LocalOnlyFact]
    public async Task ExecuteAsync_Cancellation_TerminatesProcess()
    {
        var processIdFile = Path.Combine(Path.GetTempPath(), $"fclex-process-{Guid.NewGuid():N}.pid");
        var completedFile = Path.Combine(Path.GetTempPath(), $"fclex-process-{Guid.NewGuid():N}.completed");
        using var cancellation = new CancellationTokenSource();
        Task<ProcessResult>? execution = null;
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
