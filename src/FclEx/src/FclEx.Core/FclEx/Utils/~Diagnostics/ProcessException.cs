namespace FclEx.Utils;

public class ProcessException(int exitCode, string message) : Exception(message)
{
    public int ExitCode { get; } = exitCode;
}