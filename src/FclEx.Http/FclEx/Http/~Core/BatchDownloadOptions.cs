namespace FclEx.Http;

public class BatchDownloadOptions
{
    public Uri? BaseAddress { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public HttpContent? Content { get; set; }
    public TimeSpan? ReadHeadersTimeout { get; set; }
    public int? BufferSize { get; set; }
    public TimeSpan? ReadBufferTimeout { get; set; }
    public bool ExecuteInParallel { get; set; } = true;
    public int? Concurrency { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
