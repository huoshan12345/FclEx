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
    /// Provide more information than <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="WebException"></exception>
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