namespace System.Runtime.InteropServices;

public static class OSPlatformExtensions
{
    private static readonly OSPlatform _freeBSD = OSPlatform.Create("FREEBSD");

    extension(OSPlatform)
    {
#if !NET5_0_OR_GREATER
        public static OSPlatform FreeBSD => _freeBSD;
#endif
    }
}
