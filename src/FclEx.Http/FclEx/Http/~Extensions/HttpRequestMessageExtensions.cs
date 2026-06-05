namespace FclEx.Http;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage AddCookie(this HttpRequestMessage request, string? cookie)
    {
        if (cookie.IsNotEmpty())
        {
            request.Headers.Add(HttpHeaderNames.Cookie, cookie);
        }

        return request;
    }

    private const int MessageNotYetSent = 0;

#if NET8_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sendStatus")]
    private static extern ref int HttpRequestMessageSendStatus(HttpRequestMessage uri);
#endif

    public static HttpRequestMessage SetNotSend(this HttpRequestMessage request)
    {
#if NET8_0_OR_GREATER
        ref var status = ref HttpRequestMessageSendStatus(request);
        status = MessageNotYetSent;
#else
        FieldInfos.HttpRequestMessage_SendStatus.SetValue(request, MessageNotYetSent);
#endif
        return request;
    }

    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
    {
        // 1. Clone basic request properties
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // 2. Clone headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

#if NET5_0_OR_GREATER
        // 3. Clone custom request options/properties
        foreach (var (key, value) in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(key), value);
        }
#endif

        if (request.Content == null)
            return clone;

        // 4. Deep clone HTTP Content and Content Headers
        // Read existing content into a memory stream to ensure it can be read again safely
        var ms = new MemoryStream();
        await request.Content.CopyToAsync(ms).ConfigureAwait(false);
        ms.Position = 0;

        clone.Content = new StreamContent(ms);

        // Copy content headers (e.g., Content-Type)
        foreach (var header in request.Content.Headers)
        {
            clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}