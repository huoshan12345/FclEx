namespace FclEx.Http;

public class DeflateContent : CompressedContent
{
    public DeflateContent(HttpContent content, CompressionLevel compressionLevel, 
        TimeSpan? timeout = null, int bufferSize = 262144, CancellationToken token = default)
        : base(content, compressionLevel, timeout, bufferSize, token)
    {
    }

    public override string Encoding { get; } = "deflate";

    protected override Stream CreateCompressedStream(Stream stream)
    {
        // Yes, ZLibStream over DeflateStream.
        // See this note: https://github.com/dotnet/runtime/blob/7ab969c84ef05ba948c0075392716ce335b47744/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/DecompressionHandler.cs#L231
#if NETSTANDARD2_0
        return new DeflateStream(stream, CompressionLevel, true);
#else
        return new ZLibStream(stream, CompressionLevel, true);
#endif
    }
}