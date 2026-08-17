namespace FclEx.Utils;

public class ProcessException(ProcessResult result)
    : Exception(CreateMessage(result))
{
    public ProcessResult Result { get; } = result;

    public int ExitCode => Result.ExitCode;

    private static string CreateMessage(ProcessResult result)
    {
        var details = result.StandardError.IsNotEmpty()
            ? result.StandardError
            : result.StandardOutput;
        return details.IsNotEmpty()
            ? $"The process exited with code {result.ExitCode}.{Environment.NewLine}{details}"
            : $"The process exited with code {result.ExitCode}.";
    }
}
