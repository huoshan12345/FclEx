namespace FclEx.Diagnostics;

public class WslHelper
{
    // We use instance method here to make extenstion methods possible
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public async Task<string> ExecuteCommandAsync(WslCommand command)
    {
        var text = command.StripCarriageReturn
            ? command.CommandText.Replace("\r", "")
            : command.CommandText;

        // ReSharper disable once UsingStatementResourceInitialization
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash.exe",
                Arguments = $"-c \"{text}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = command.WorkingDirectory ?? "/",
                StandardOutputEncoding = command.OutputEncoding,
                StandardErrorEncoding = command.OutputEncoding,
            },
            EnableRaisingEvents = true,
        };

        var output = await process.GetOutput();
        return output;
    }
}