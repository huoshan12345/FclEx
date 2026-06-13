namespace FclEx.Http;

/// <summary>
/// Determines how a response body is exposed on <see cref="HttpResponse"/>.
/// </summary>
public enum HttpContentType
{
    /// <summary>Decode the response body into <see cref="HttpResponse.ResponseString"/>.</summary>
    String,
    /// <summary>Read the response body into <see cref="HttpResponse.ResponseBytes"/>.</summary>
    Bytes,
    /// <summary>Expose the response body as <see cref="HttpResponse.ResponseStream"/>.</summary>
    Stream,
}
