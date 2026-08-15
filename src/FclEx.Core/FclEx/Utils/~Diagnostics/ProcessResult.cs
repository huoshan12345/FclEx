namespace FclEx.Utils;

/// <summary>
/// Contains the exit code and independently captured output streams of a completed process.
/// </summary>
/// <param name="ExitCode">The process exit code.</param>
/// <param name="StandardOutput">The text written to standard output.</param>
/// <param name="StandardError">The text written to standard error.</param>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>Gets whether the process exited with code zero.</summary>
    public bool Succeeded => ExitCode == 0;
}
