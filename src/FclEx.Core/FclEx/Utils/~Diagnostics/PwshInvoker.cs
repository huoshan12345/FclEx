namespace FclEx.Utils;

public class PwshInvoker : PowerShellInvoker
{
    /// <summary>
    /// create custom pwsh process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="commandArgumentFactory"></param>
    public PwshInvoker(string fileName, Func<string, IReadOnlyList<string>> commandArgumentFactory)
        : base(fileName, commandArgumentFactory)
    {
    }

    /// <summary>
    /// create default pwsh process.
    /// </summary>
    public PwshInvoker() : this("pwsh", text => ["-NoLogo", "-NoProfile", "-NonInteractive", "-Command", text])
    {
    }

    public new static readonly PwshInvoker Instance = new();
}
