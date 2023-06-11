using System.Diagnostics.CodeAnalysis;
using Microsoft.Collections.Extensions;

namespace FclEx.Http;

public class HttpRes
{
    public HttpRes(HttpReq req)
    {
        HttpReq = req ?? throw new ArgumentNullException(nameof(req));
    }
        
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool HasError => Exception != null;
    public Exception? Exception { get; internal set; }

    public HttpReq HttpReq { get; }
    public string ResponseString { get; internal set; } = string.Empty;
    public Encoding? Encoding { get; internal set; }
    public byte[] ResponseBytes { get; internal set; } = Array.Empty<byte>();
    public TimeSpan ExecuteTime { get; internal set; }
    public DateTime RequestUtcTime { get; internal set; }
    public MultiValueDictionary<string, string?> Headers { get; } = new(StringComparer.InvariantCultureIgnoreCase);
    public HttpStatusCode StatusCode { get; internal set; }
    public List<Uri> RedirectUris { get; } = new();

    internal static readonly HttpRes EmptyRes = new(new HttpReq(string.Empty, HttpMethodType.Get));
    public static HttpRes CreateError(HttpReq req, Exception e) => new(req) { Exception = e };
}