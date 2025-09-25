namespace FclEx.Utils;

public class ProcessRunnerOptions
{
    public required string FileName { get; set; }
    public string WorkingDirectory { get; set; } = "";
}

public class ProcessRunner
{
    private readonly ProcessRunnerOptions _options;
    public Process Process { get; }

    public ProcessRunner(ProcessRunnerOptions options)
    {
        _options = options;
        Process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = options.FileName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = options.WorkingDirectory,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            },
            EnableRaisingEvents = true,
        };
        Process.Start();
    }


}
