namespace FclEx.Utils;

public class ProcessRunner
{
    public static readonly PowerShellRunner PowerShell = PowerShellRunner.Instance;
    public static readonly PowerShellRunner Pwsh = PwshRunner.Instance;
    public static readonly WslRunner Wsl = WslRunner.Instance;

    private readonly string _fileName;
    private readonly Func<string, string> _argumentsConverter;

    public ProcessRunner(string fileName, Func<string, string> argumentsConverter)
    {
        _fileName = fileName;
        _argumentsConverter = argumentsConverter;
    }

    public async Task<string> ExecuteAsync(ProcessCommand command)
    {
        var text = command.StripCarriageReturn
            ? command.CommandText.Replace("\r", "")
            : command.CommandText;

        var arguments = _argumentsConverter(text);

        // ReSharper disable once UsingStatementResourceInitialization
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _fileName,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = command.WorkingDirectory ?? "/",
                StandardOutputEncoding = command.OutputEncoding ?? Encoding.UTF8,
                StandardErrorEncoding = command.ErrorEncoding ?? command.OutputEncoding ?? Encoding.UTF8,
            },
            EnableRaisingEvents = true,

        };
        var queue = new ConcurrentQueue<string?>();
        process.OutputDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.ErrorDataReceived += (sender, e) => queue.Enqueue(e.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(command.CancellationToken);

        var output = queue.Where(m => m is not null).JoinWith(Environment.NewLine);

        if (process.ExitCode != 0 && command.IgnoreNonZeroExitCode == false)
            throw new ProcessException(process.ExitCode, output);

        return output;
    }
}