namespace FclEx.Utils;

public class ProcessInvoker(string fileName, Func<string, IReadOnlyList<string>> commandArgumentFactory)
{
    public static readonly PowerShellInvoker PowerShell = PowerShellInvoker.Instance;
    public static readonly PwshInvoker Pwsh = PwshInvoker.Instance;
    public static readonly WslInvoker Wsl = WslInvoker.Instance;

    public async Task<ProcessResult> ExecuteAsync(ProcessInvocation invocation)
    {
        var text = invocation.StripCarriageReturn
            ? invocation.CommandText.Replace("\r", "")
            : invocation.CommandText;

        var arguments = commandArgumentFactory(text);
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            WorkingDirectory = invocation.WorkingDirectory ?? Environment.CurrentDirectory,
            StandardOutputEncoding = invocation.OutputEncoding ?? Encoding.UTF8,
            StandardErrorEncoding = invocation.ErrorEncoding ?? invocation.OutputEncoding ?? Encoding.UTF8,
        };

#if NET5_0_OR_GREATER
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);
#else
        startInfo.Arguments = arguments.Select(QuoteArgument).JoinWith(" ");
#endif

        // ReSharper disable once UsingStatementResourceInitialization
        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        var standardOutput = new ConcurrentQueue<string>();
        var standardError = new ConcurrentQueue<string>();
        var outputCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var errorCompleted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.OutputDataReceived += (_, e) => CaptureOutput(e, standardOutput, outputCompleted);
        process.ErrorDataReceived += (_, e) => CaptureOutput(e, standardError, errorCompleted);
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

        var result = new ProcessResult(
            process.ExitCode,
            standardOutput.JoinWith(Environment.NewLine),
            standardError.JoinWith(Environment.NewLine));

        if (!result.Succeeded && invocation.ExitCodePolicy == ProcessExitCodePolicy.Throw)
            throw new ProcessException(result);

        return result;

        static void CaptureOutput(
            DataReceivedEventArgs args,
            ConcurrentQueue<string> destination,
            TaskCompletionSource<object?> completion)
        {
            if (args.Data is null)
                completion.TrySetResult(null);
            else
                destination.Enqueue(args.Data);
        }
    }

#if !NET5_0_OR_GREATER
    // ProcessStartInfo.ArgumentList is unavailable on the legacy targets. This is the same
    // backslash-and-quote encoding used by CommandLineToArgvW and by .NET's process launcher.
    private static string QuoteArgument(string argument)
    {
        if (argument.Length != 0 && argument.All(c => !char.IsWhiteSpace(c) && c != '"'))
            return argument;

        var result = new StringBuilder(argument.Length + 2);
        result.Append('"');
        var backslashes = 0;

        foreach (var c in argument)
        {
            if (c == '\\')
            {
                backslashes++;
                continue;
            }

            if (c == '"')
                result.Append('\\', backslashes * 2 + 1);
            else
                result.Append('\\', backslashes);

            backslashes = 0;
            result.Append(c);
        }

        result.Append('\\', backslashes * 2);
        result.Append('"');
        return result.ToString();
    }
#endif
}
