namespace FclEx.Http;

public class DownloadOptions
{
    public required Uri Uri { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public HttpContent? Content { get; set; }
    public TimeSpan? ConnectTimeout { get; set; }
    public TimeSpan? ReadBufferTimeout { get; set; }
    public CancellationToken CancellationToken { get; set; } = default;
    public string? FileBaseName { get; set; }
    public string? FileExtension { get; set; }
}