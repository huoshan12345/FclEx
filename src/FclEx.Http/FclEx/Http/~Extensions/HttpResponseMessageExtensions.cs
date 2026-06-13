namespace FclEx.Http;

public static class HttpResponseMessageExtensions
{
    public static bool TryGetRedirection(this HttpResponseMessage response, [NotNullWhen(true)] out Uri? uri)
    {
        if (response.StatusCode.IsRedirection() && response.Headers.Location is { } u)
        {
            if (u.IsAbsoluteUri)
            {
                uri = u;
                return true;
            }

            if (response.RequestMessage?.RequestUri is { } baseUri)
            {
                uri = new Uri(baseUri, u);
                return true;
            }
        }

        uri = null;
        return false;
    }

    /// <summary>
    /// Throws when the response status code is outside the successful 2xx range.
    /// </summary>
    /// <param name="response">The response to validate.</param>
    /// <returns>The same response when the status code is successful.</returns>
    /// <exception cref="HttpRequestException">
    /// Thrown for non-successful status codes. The exception includes the numeric status code and,
    /// when available, the original request method and URI.
    /// </exception>
    public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage response)
    {
        var request = response.RequestMessage;
        response.StatusCode.EnsureSuccess(request?.RequestUri, request?.Method.Method);
        return response;
    }

    public static bool TryGetCookies(this HttpResponseMessage response, [NotNullWhen(true)] out IEnumerable<string>? cookies)
    {
        return response.Headers.TryGetValues(HttpHeaderNames.SetCookie, out cookies);
    }
}
