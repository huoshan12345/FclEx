namespace FclEx.Http;

public static class HttpMessageHandlerExtensions
{
    public static IEnumerable<HttpMessageHandler> EnumerateInner(this HttpMessageHandler handler)
    {
        var p = handler;
        while (p != null)
        {
            yield return p;

            if (p is DelegatingHandler delegatingHandler)
                p = delegatingHandler.InnerHandler;
            else
                break;
        }
    }
}