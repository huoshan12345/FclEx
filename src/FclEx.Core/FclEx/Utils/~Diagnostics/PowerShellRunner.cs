namespace FclEx.Utils;

public class PowerShellRunner : ProcessRunner
{
    /// <summary>
    /// create custom powershell process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="argumentsConverter"></param>
    public PowerShellRunner(string fileName, Func<string, string> argumentsConverter)
        : base(fileName, argumentsConverter)
    {
    }

    /// <summary>
    /// create default powershell process.
    /// </summary>
    public PowerShellRunner() : this("powershell", text => $"-command \"{text}\"")
    {
    }

    public static readonly PowerShellRunner Instance = new();
}
