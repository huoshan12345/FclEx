using System.Net.Http;
using System.Threading;

namespace FclEx.Http;

public class ArraySegmentContent : HttpContent
{
    private readonly ArraySegment<byte> _content;
    private readonly CancellationToken _token;
    private readonly TimeSpan? _timeout;

    public ArraySegmentContent(ArraySegment<byte> content, TimeSpan? timeout, CancellationToken token)
    {
        Check.NotNull(content.Array!);
        _content = content;
        _timeout = timeout;
        _token = token;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        await using var ms = CreateContentReadStream();
        await ms.CopyToAsync(dest: stream, readBufferTimeout: _timeout, token: _token);
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _content.Count;
        return true;
    }

    protected override Task<Stream> CreateContentReadStreamAsync()
    {
        return CreateContentReadStream().ToTask();
    }

    private Stream CreateContentReadStream()
    {
        return new MemoryStream(_content.Array!, _content.Offset, _content.Count, false);
    }
}