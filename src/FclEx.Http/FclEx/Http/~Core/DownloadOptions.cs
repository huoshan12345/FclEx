namespace FclEx.Http;

public class DownloadOptions
{
    public required Uri Uri { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public HttpContent? Content { get; set; }
    public int? BufferSize { get; set; }
    public TimeSpan? ReadHeadersTimeout { get; set; }
    public TimeSpan? ReadBufferTimeout { get; set; }
    public CancellationToken CancellationToken { get; set; } = default;
    public string? FileBaseName { get; set; }
    public string? FileExtension { get; set; }
    public bool DisposeContent { get; set; } = true;
}