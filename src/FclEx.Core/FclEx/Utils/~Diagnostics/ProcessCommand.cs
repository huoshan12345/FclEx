namespace FclEx.Utils;

public class ProcessCommand(Process process, string commandText, bool stripCarriageReturn = true)
{
    public Process Process { get; } = process;

    public string CommandText { get; } = stripCarriageReturn
        ? commandText.Replace("\r", "")
        : commandText;

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await Process.StandardInput.WriteLineAsync(CommandText);
        await Process.StandardInput.FlushAsync(cancellationToken);
        return await Process.StandardOutput.ReadToEndAsync(cancellationToken);
    }
}
