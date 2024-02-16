namespace FclEx.Http;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage AddCookie(this HttpRequestMessage request, string? cookie)
    {
        if (cookie.IsNotEmpty())
        {
            request.Headers.Add(HttpKnownHeaderNames.Cookie, cookie);
        }

        return request;
    }
}