namespace FclEx.Utils;

public class WslRunner : ProcessRunner
{
    /// <summary>
    /// create custom wsl process
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="argumentsConverter"></param>
    public WslRunner(string fileName, Func<string, string> argumentsConverter) 
        : base(fileName, argumentsConverter)
    {
    }

    /// <summary>
    /// create wsl process with default bash
    /// </summary>
    public WslRunner() : this("bash", text => $"-c \"{text}\"")
    {
    }

    public static readonly WslRunner Instance = new();
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
    public static Task<string> WslPath(this WslRunner wsl, string path, string? options = null, Encoding? outputEncoding = null, CancellationToken cancellationToken = default)
    {
        return wsl.ExecuteAsync(new ProcessCommand($"wslpath '{path}' {options}", OutputEncoding: outputEncoding, CancellationToken: cancellationToken));
    }
}