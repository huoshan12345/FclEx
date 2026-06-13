namespace FclEx.Http;

/// <summary>
/// Extensions for inspecting HTTP message handler chains.
/// </summary>
public static class HttpMessageHandlerExtensions
{
    /// <summary>
    /// Enumerates a handler and each nested <see cref="DelegatingHandler.InnerHandler"/> until the primary handler is reached.
    /// </summary>
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
