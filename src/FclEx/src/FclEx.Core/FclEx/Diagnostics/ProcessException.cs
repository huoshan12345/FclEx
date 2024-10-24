namespace FclEx.Diagnostics;

public class ProcessException : Exception
{
    public ProcessException(int exitCode, string message) : base(message)
    {
        ExitCode = exitCode;
    }

    public int ExitCode { get; }
}