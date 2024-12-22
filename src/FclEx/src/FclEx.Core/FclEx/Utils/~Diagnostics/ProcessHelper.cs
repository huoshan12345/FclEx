namespace FclEx.Utils;

public static class WslHelperExtensions
{
    /// <summary>
    /// Convert paths between Windows and Linux. <br/>
    /// The options are: <br/>
    /// -a force result to absolute path format. <br/>
    /// -u translate from a Windows path to a WSL path (default). <br/>
    /// -w translate from a WSL path to a Windows path. <br/>
    /// -m translate from a WSL path to a Windows path, with ¡®/¡¯ instead of ¡®\\¡¯.
    /// </summary>
    /// <param name="wsl"></param>
    /// <param name="path"></param>
    /// <param name="options"></param>
    /// <param name="outputEncoding"></param>
    /// <returns></returns>
    public static Task<string> WslPath(this WslHelper wsl, string path, string? options = null, Encoding? outputEncoding = null)
    {
        return wsl.ExecuteCommandAsync(new WslCommand { CommandText = $"wslpath '{path}' {options}", OutputEncoding = outputEncoding });
    }
}

public static class ProcessHelper
{
    public static WslHelper Wsl { get; } = new();
}