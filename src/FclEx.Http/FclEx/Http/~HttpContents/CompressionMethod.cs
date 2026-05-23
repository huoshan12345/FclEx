namespace FclEx.Http;

public enum CompressionMethod
{
    /// <summary>Do not use compression.</summary>
    None,
    /// <summary>Use the gZip compression-decompression algorithm.</summary>
    GZip = 1,
    /// <summary>Use the deflate compression-decompression algorithm.</summary>
    Deflate = 2,
#if NET6_0_OR_GREATER
    /// <summary>Use the Brotli compression-decompression algorithm.</summary>
    Brotli = 4,
#endif
}