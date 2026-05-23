namespace FclEx.Utils;

public record ProcessInvocation(
    string CommandText,
    string? WorkingDirectory = null,
    bool StripCarriageReturn = true,
    Encoding? OutputEncoding = null,
    Encoding? ErrorEncoding = null,
    bool IgnoreNonZeroExitCode = false,
    CancellationToken CancellationToken = default)
{
    public static implicit operator ProcessInvocation(string commandText) => new(commandText);
}