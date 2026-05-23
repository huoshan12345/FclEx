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

        var queue = new ConcurrentQueue<string?>();
        process.OutputDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.ErrorDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(invocation.CancellationToken);

        var output = queue.Where(m => m is not null).JoinWith(Environment.NewLine);

        if (process.ExitCode != 0 && invocation.IgnoreNonZeroExitCode == false)
            throw new ProcessException(process.ExitCode, output);

        return output;
    }
}