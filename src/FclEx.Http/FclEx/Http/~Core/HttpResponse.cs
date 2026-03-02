namespace FclEx.Http;

public class HttpResponse
{
    public HttpResponse(HttpRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => Exception is null;

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsError => Exception != null;
    public Exception? Exception { get; internal set; }

    public HttpRequest Request { get; }
    public string ResponseString { get; internal set; } = string.Empty;
    public byte[] ResponseBytes { get; internal set; } = [];
    public Stream ResponseStream { get; internal set; } = new MemoryStream();
    public Encoding? Encoding { get; internal set; }
    public TimeSpan Elapsed { get; internal set; }
    public DateTimeOffset StartTime { get; internal set; }
    public DateTimeOffset EndTime => StartTime + Elapsed;
    public MultiValueDictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HttpStatusCode StatusCode { get; internal set; }
    public List<Uri> RedirectUris { get; } = [];

    public static HttpResponse FromError(HttpRequest request, Exception ex) => new(request) { Exception = ex };
}