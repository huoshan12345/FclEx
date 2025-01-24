namespace FclEx.Utils;

public record ProcessCommand(
    string CommandText,
    string? WorkingDirectory = null,
    bool StripCarriageReturn = true,
    Encoding? OutputEncoding = null,
    Encoding? ErrorEncoding = null,
    bool IgnoreNonZeroExitCode = false,
    CancellationToken CancellationToken = default)
{
    public static implicit operator ProcessCommand(string commandText) => new(commandText);
}