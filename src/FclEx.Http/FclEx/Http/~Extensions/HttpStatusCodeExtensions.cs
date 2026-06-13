namespace FclEx.Http;

/// <summary>
/// Extensions for classifying and validating HTTP status codes.
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    /// Returns whether the code is in the 1xx informational range.
    /// </summary>
    public static bool IsInfo(this HttpStatusCode code) => (int)code >= 100 && (int)code <= 199;

    /// <summary>
    /// Returns whether the code is in the 2xx successful range.
    /// </summary>
    public static bool IsSuccess(this HttpStatusCode code) => (int)code >= 200 && (int)code <= 299;

    /// <summary>
    /// Returns whether the code is in the 3xx redirection range.
    /// </summary>
    public static bool IsRedirection(this HttpStatusCode code) => (int)code >= 300 && (int)code <= 399;

    /// <summary>
    /// Returns whether the code is in the 4xx client-error range.
    /// </summary>
    public static bool IsClientError(this HttpStatusCode code) => (int)code >= 400 && (int)code <= 499;

    /// <summary>
    /// Returns whether the code is in the 5xx server-error range.
    /// </summary>
    public static bool IsServerError(this HttpStatusCode code) => (int)code >= 500 && (int)code <= 599;

    /// <summary>
    /// Returns whether this redirect status code preserves the original HTTP method and request content.
    /// </summary>
    /// <remarks>
    /// This applies to 307 Temporary Redirect and 308 Permanent Redirect.<br/>
    /// Unlike 301, 302, and 303, these status codes require the redirected request to use the same method
    /// and keep the original request content.
    /// </remarks>
    public static bool PreservesMethodAndContent(this HttpStatusCode statusCode)
    {
        return (int)statusCode is 307 or 308;
    }

    /// <summary>
    /// Returns the HTTP status-code class represented by the first digit of the numeric status code.
    /// Values outside 100 through 599 are classified as <see cref="HttpStatusCodeClass.Unknown"/>.
    /// </summary>
    public static HttpStatusCodeClass GetStatusCodeClass(this HttpStatusCode code)
    {
        var digit = ((int)code) / 100;
        return digit is >= 1 and <= 5
            ? (HttpStatusCodeClass)digit
            : HttpStatusCodeClass.Unknown;
    }

    private static readonly ConcurrentDictionary<HttpStatusCode, string> _cache = new();

    /// <summary>
    /// Formats a status code as <c>Name/Number</c> and caches the formatted value.
    /// </summary>
    public static string ToPairString(this HttpStatusCode code)
    {
        return _cache.GetOrAdd(code, m => $"{m}/{m.ToInt()}");
    }

    /// <summary>
    /// Throws an <see cref="HttpRequestException"/> when the status code is outside the 2xx successful range.
    /// The message includes the request method and URI when supplied.
    /// </summary>
    public static void EnsureSuccess(this HttpStatusCode code, Uri? uri, string? method)
    {
        if (code.IsSuccess())
            return;

        var error = $"Returned {code.ToPairString()} via {method} {uri}";
        throw HttpRequestException.From(error, null, code);
    }
}
