namespace FclEx.Http;

public class BatchDownloadOptions
{
    public Uri? BaseAddress { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public HttpContent? Content { get; set; }
    public TimeSpan? ConnectTimeout { get; set; }
    public TimeSpan? ReadBufferTimeout { get; set; }
    public CancellationToken CancellationToken { get; set; }
}