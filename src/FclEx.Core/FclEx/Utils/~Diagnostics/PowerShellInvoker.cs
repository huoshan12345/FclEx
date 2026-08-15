namespace FclEx.Utils;

public class PowerShellInvoker : ProcessInvoker
{
    /// <summary>
    /// create custom powershell process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="commandArgumentFactory"></param>
    public PowerShellInvoker(string fileName, Func<string, IReadOnlyList<string>> commandArgumentFactory)
        : base(fileName, commandArgumentFactory)
    {
    }

    /// <summary>
    /// create default powershell process.
    /// </summary>
    public PowerShellInvoker() : this("powershell", text => ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command", text])
    {
    }

    public static readonly PowerShellInvoker Instance = new();
}
