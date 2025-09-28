namespace FclEx.Utils;

public class ProcessRunnerOptions
{
    public required string FileName { get; set; }
    public string WorkingDirectory { get; set; } = "";
}

public class ProcessRunner : IDisposable
{
    public Process Process { get; }

    public ProcessRunner(string fileName) : this(new ProcessRunnerOptions { FileName = fileName }) { }

    public ProcessRunner(ProcessRunnerOptions options)
    {
        Process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = options.FileName,
                RedirectStandardOutput = true,
                // RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = options.WorkingDirectory,
#if NET5_0_OR_GREATER
                StandardInputEncoding = Encoding.UTF8,
#endif
                //StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
            },
            EnableRaisingEvents = true,
        };
        Process.Start();
    }

    public async Task<string> ExecuteAsync(string commandText, CancellationToken cancellationToken = default)
    {
        await Process.StandardInput.WriteLineAsync(commandText);
        await Process.StandardInput.FlushAsync(cancellationToken);

        var builder = new StringBuilder();
        while (true)
        {
            var line = await Process.StandardOutput.ReadLineAsync(cancellationToken);
            if (line == null) 
                break;

            builder.AppendLine(line);
        }

        return builder.ToString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Process.Dispose();
    }
}
