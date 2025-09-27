namespace FclEx.Utils;

public class PwshInvoker : PowerShellInvoker
{    /// <summary>
    /// create custom pwsh process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="argumentsConverter"></param>
    public PwshInvoker(string fileName, Func<string, string> argumentsConverter)
        : base(fileName, argumentsConverter)
    {
    }

    /// <summary>
    /// create default pwsh process.
    /// </summary>
    public PwshInvoker() : this("pwsh", text => $"-command \"{text}\"")
    {
    }

    public new static readonly PwshInvoker Instance = new("pwsh", text => $"-command \"{text}\"");
}
