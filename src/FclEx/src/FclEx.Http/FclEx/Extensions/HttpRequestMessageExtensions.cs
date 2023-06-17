namespace FclEx.Extensions;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage AddCookie(this HttpRequestMessage request, string? cookie)
    {
        if (cookie.IsValid())
        {
            request.Headers.Add(HttpKnownHeaderNames.Cookie, cookie);
        }

        return request;
    }
}