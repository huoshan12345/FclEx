namespace FclEx.Utils;

public enum OSPlatformType
{
    Windows,
    Linux,
    OSX,
    FreeBSD,
}

public static class OSPlatformTypeExtensions
{
    public static OSPlatform FreeBSD { get; } =
#if NETSTANDARD2_0
        OSPlatform.Create("FREEBSD");
#else
        OSPlatform.FreeBSD;
#endif
    public static OSPlatform ToOSPlatform(this OSPlatformType type)
    {
        return type switch
        {
            OSPlatformType.Windows => OSPlatform.Windows,
            OSPlatformType.Linux => OSPlatform.Linux,
            OSPlatformType.OSX => OSPlatform.OSX,
            OSPlatformType.FreeBSD => FreeBSD,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}