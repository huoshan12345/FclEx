namespace FclEx.Http;

/// <summary>
/// Represents the result of sending an <see cref="HttpRequest"/>.
/// The response stores either a transport/processing exception or the final HTTP response data collected after redirects.
/// </summary>
public class HttpResponse
{
    /// <summary>
    /// Creates a response associated with the request that produced it.
    /// </summary>
    public HttpResponse(HttpRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    /// <summary>
    /// Indicates that the request completed without a captured exception.
    /// A non-success HTTP status code can still be represented as success unless the caller requested status-code enforcement.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool IsSuccess => Exception is null;

    /// <summary>
    /// Indicates that sending, redirect handling, content reading, or status-code enforcement captured an exception.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool IsError => Exception != null;

    /// <summary>
    /// Exception captured while processing the request, or <see langword="null"/> when the request completed normally.
    /// </summary>
    public Exception? Exception { get; internal set; }

    /// <summary>
    /// The request that produced this response.
    /// </summary>
    public HttpRequest Request { get; }

    /// <summary>
    /// Response body decoded as a string when the request was configured to read string content.
    /// </summary>
    public string ResponseString { get; internal set; } = string.Empty;

    /// <summary>
    /// Response body bytes when the request was configured to read byte content.
    /// </summary>
    public byte[] ResponseBytes { get; internal set; } = [];

    /// <summary>
    /// Response body stream when the request was configured to read stream content.
    /// Disposing this stream also disposes the underlying <see cref="HttpResponseMessage"/>.
    /// </summary>
    public Stream ResponseStream { get; internal set; } = Stream.Null;

    /// <summary>
    /// Encoding used when <see cref="ResponseString"/> was decoded.
    /// </summary>
    public Encoding? Encoding { get; internal set; }

    /// <summary>
    /// Elapsed time measured for the request workflow.
    /// </summary>
    public TimeSpan Elapsed { get; internal set; }

    /// <summary>
    /// Time at which the request workflow started.
    /// </summary>
    public DateTimeOffset StartTime { get; internal set; }

    /// <summary>
    /// Time at which the request workflow ended, computed from <see cref="StartTime"/> and <see cref="Elapsed"/>.
    /// </summary>
    public DateTimeOffset EndTime => StartTime + Elapsed;

    /// <summary>
    /// Response headers collected from the final response, plus cookies added through helper methods.
    /// Header names are compared case-insensitively.
    /// </summary>
    public MultiValueDictionary<string, string?> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Status code from the final response.
    /// </summary>
    public HttpStatusCode StatusCode { get; internal set; }

    /// <summary>
    /// URIs visited while sending the request, including redirects.
    /// </summary>
    public List<Uri> VisitedUris { get; } = [];

    /// <summary>
    /// Creates an error response for a request.
    /// </summary>
    public static HttpResponse FromError(HttpRequest request, Exception ex) => new(request) { Exception = ex };
}
