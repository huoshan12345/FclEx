namespace FclEx.Utils;

/// <summary>
/// Specifies how an invocation handles a process that exits with a non-zero exit code.
/// </summary>
public enum ProcessExitCodePolicy
{
    /// <summary>Throw a <see cref="ProcessException"/> containing the process result.</summary>
    Throw,

    /// <summary>Return the process result to the caller.</summary>
    ReturnResult,
}

public sealed record ProcessInvocation(
    string CommandText,
    string? WorkingDirectory = null,
    bool StripCarriageReturn = true,
    Encoding? OutputEncoding = null,
    Encoding? ErrorEncoding = null,
    ProcessExitCodePolicy ExitCodePolicy = ProcessExitCodePolicy.Throw,
    CancellationToken CancellationToken = default)
{
    public static implicit operator ProcessInvocation(string commandText) => new(commandText);
}
