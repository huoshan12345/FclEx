namespace FclEx.Utils;

public class ProcessInvoker(string fileName, Func<string, string> argumentsConverter)
{
    public static readonly PowerShellInvoker PowerShell = PowerShellInvoker.Instance;
    public static readonly PowerShellInvoker Pwsh = PwshInvoker.Instance;
    public static readonly WslInvoker Wsl = WslInvoker.Instance;

    public async Task<string> ExecuteAsync(ProcessInvocation invocation)
    {
        var text = invocation.StripCarriageReturn
            ? invocation.CommandText.Replace("\r", "")
            : invocation.CommandText;

        var arguments = argumentsConverter(text);

        // ReSharper disable once UsingStatementResourceInitialization
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = invocation.WorkingDirectory ?? "/",
                StandardOutputEncoding = invocation.OutputEncoding ?? Encoding.UTF8,
                StandardErrorEncoding = invocation.ErrorEncoding ?? invocation.OutputEncoding ?? Encoding.UTF8,
            },
            EnableRaisingEvents = true,
        };

        var queue = new ConcurrentQueue<string>();
        var outputCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errorCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.OutputDataReceived += (_, e) => CaptureOutput(e, outputCompleted);
        process.ErrorDataReceived += (_, e) => CaptureOutput(e, errorCompleted);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(invocation.CancellationToken);
        }
        catch (OperationCanceledException) when (invocation.CancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!process.HasExited)
                {
#if NET5_0_OR_GREATER
                    process.Kill(true);
#else
                    process.Kill();
#endif
                }

                await process.WaitForExitAsync();
            }
            catch (InvalidOperationException)
            {
                // The process exited between the HasExited check and Kill.
            }

            await Task.WhenAll(outputCompleted.Task, errorCompleted.Task);

            throw;
        }

        await Task.WhenAll(outputCompleted.Task, errorCompleted.Task);

        var output = queue.JoinWith(Environment.NewLine);

        if (process.ExitCode != 0 && invocation.IgnoreNonZeroExitCode == false)
            throw new ProcessException(process.ExitCode, output);

        return output;

        void CaptureOutput(DataReceivedEventArgs args, TaskCompletionSource<object?> completion)
        {
            if (args.Data is null)
                completion.TrySetResult(null);
            else
                queue.Enqueue(args.Data);
        }
    }
}
