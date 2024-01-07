namespace FclEx.Utils;

public enum OSPlatformType
{
    Windows,
    Linux,
    OSX,
    FreeBSD
}

public static class OSPlatformTypeExtensions
{
    public static OSPlatform ToOSPlatform(this OSPlatformType type)
    {
        return type switch
        {
            OSPlatformType.Windows => OSPlatform.Windows,
            OSPlatformType.Linux => OSPlatform.Linux,
            OSPlatformType.OSX => OSPlatform.OSX,
            OSPlatformType.FreeBSD => OSPlatform.FreeBSD,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}