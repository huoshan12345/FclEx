namespace FclEx.Utils;

public class WslInvoker : ProcessInvoker
{
    /// <summary>
    /// create custom wsl process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="commandArgumentFactory"></param>
    public WslInvoker(string fileName, Func<string, IReadOnlyList<string>> commandArgumentFactory)
        : base(fileName, commandArgumentFactory)
    {
    }

    /// <summary>
    /// create wsl process with default bash
    /// </summary>
    public WslInvoker() : this("bash", text => ["-c", text])
    {
    }

    public static readonly WslInvoker Instance = new();
}

public static class WslRunnerExtensions
{
    /// <summary>
    /// Convert paths between Windows and Linux. <br/>
    /// The options are: <br/>
    /// -a force result to absolute path format. <br/>
    /// -u translate from a Windows path to a WSL path (default). <br/>
    /// -w translate from a WSL path to a Windows path. <br/>
    /// -m translate from a WSL path to a Windows path, with ‘/’ instead of ‘\\’.
    /// </summary>
    public static async Task<string> WslPath(this WslInvoker wsl, string path, string? options = null, Encoding? outputEncoding = null, CancellationToken cancellationToken = default)
    {
        var escapedPath = path.Replace("'", "'\\''");
        var result = await wsl.ExecuteAsync(new ProcessInvocation($"wslpath '{escapedPath}' {options}", OutputEncoding: outputEncoding, CancellationToken: cancellationToken));
        return result.StandardOutput;
    }
}
