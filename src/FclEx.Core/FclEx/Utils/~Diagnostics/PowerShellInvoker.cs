namespace FclEx.Utils;

public class PowerShellInvoker : ProcessInvoker
{
    /// <summary>
    /// create custom powershell process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="argumentsConverter"></param>
    public PowerShellInvoker(string fileName, Func<string, string> argumentsConverter)
        : base(fileName, argumentsConverter)
    {
    }

    /// <summary>
    /// create default powershell process.
    /// </summary>
    public PowerShellInvoker() : this("powershell", text => $"-command \"{text}\"")
    {
    }

    public static readonly PowerShellInvoker Instance = new();
}
