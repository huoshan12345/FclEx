using Microsoft.Collections.Extensions;

namespace FclEx.Http;

public class HttpResponse
{
    public HttpResponse(HttpRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HasError => Exception != null;
    public Exception? Exception { get; internal set; }

    public HttpRequest Request { get; }
    public string ResponseString { get; internal set; } = string.Empty;
    public byte[] ResponseBytes { get; internal set; } = Array.Empty<byte>();
    public Stream ResponseStream { get; internal set; } = new MemoryStream(Array.Empty<byte>());
    public Encoding? Encoding { get; internal set; }
    public TimeSpan ExecuteTime { get; internal set; }
    public DateTime RequestUtcTime { get; internal set; }
    public MultiValueDictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HttpStatusCode StatusCode { get; internal set; }
    public List<Uri> RedirectUris { get; } = new();

    internal static readonly HttpResponse EmptyRes = new(HttpRequest.Create(string.Empty, HttpMethod.Get));
    public static HttpResponse CreateError(HttpRequest req, Exception e) => new(req) { Exception = e };
}