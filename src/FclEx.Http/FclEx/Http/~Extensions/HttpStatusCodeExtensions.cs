namespace FclEx.Http;

public static class HttpStatusCodeExtensions
{
    public static bool IsInfo(this HttpStatusCode code) => (int)code >= 100 && (int)code <= 199;
    public static bool IsSuccess(this HttpStatusCode code) => (int)code >= 200 && (int)code <= 299;
    public static bool IsRedirection(this HttpStatusCode code) => (int)code >= 300 && (int)code <= 399;
    public static bool IsClientError(this HttpStatusCode code) => (int)code >= 400 && (int)code <= 499;
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

    public static HttpStatusCodeClass GetStatusCodeClass(this HttpStatusCode code)
    {
        var digit = ((int)code) / 100;
        return digit is >= 1 and <= 5
            ? (HttpStatusCodeClass)digit
            : HttpStatusCodeClass.Unknown;
    }

    private static readonly ConcurrentDictionary<HttpStatusCode, string> _cache = new();
    public static string ToPairString(this HttpStatusCode code)
    {
        return _cache.GetOrAdd(code, m => $"{m}/{m.ToInt()}");
    }

    public static void EnsureSuccess(this HttpStatusCode code, Uri? uri, string? method)
    {
        if (code.IsSuccess())
            return;

        var error = $"Returned {code.ToPairString()} via {method} {uri}";
        throw HttpRequestException.From(error, null, code);
    }
}