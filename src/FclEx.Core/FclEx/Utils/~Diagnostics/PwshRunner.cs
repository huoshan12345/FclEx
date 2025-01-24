namespace FclEx.Utils;

public class PwshRunner : PowerShellRunner
{    /// <summary>
    /// create custom pwsh process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="argumentsConverter"></param>
    public PwshRunner(string fileName, Func<string, string> argumentsConverter)
        : base(fileName, argumentsConverter)
    {
    }

    /// <summary>
    /// create default pwsh process.
    /// </summary>
    public PwshRunner() : this("pwsh", text => $"-command \"{text}\"")
    {
    }

    public new static readonly PwshRunner Instance = new("pwsh", text => $"-command \"{text}\"");
}
