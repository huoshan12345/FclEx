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

    /// <summary>
    /// Resets the internal send status so the same request message can be sent again.
    /// </summary>
    /// <remarks>
    /// This method relies on private runtime implementation details and may break when .NET changes
    /// <see cref="HttpRequestMessage"/> internals. Prefer <see cref="CloneAsync"/> when a reusable request is needed.
    /// </remarks>
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

    /// <summary>
    /// Creates a reusable copy of an HTTP request message, including headers, options/properties, and buffered content.
    /// </summary>
    /// <param name="request">The request message to clone.</param>
    /// <returns>A new request message whose content can be sent independently from the original request.</returns>
    /// <remarks>The original content is buffered into memory, so this is not suitable for very large streaming payloads.</remarks>
    public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
    {
        // 1. Clone basic request properties
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
#if NET5_0_OR_GREATER
            VersionPolicy = request.VersionPolicy,
#endif
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
            clone.Options.Set(key, value);
        }
#else
        // 3. Clone custom request options/properties
        foreach (var (key, value) in request.Properties)
        {
            clone.Properties[key] = value;
        }
#endif

        if (request.Content == null)
            return clone;

        // 4. Deep clone HTTP Content and Content Headers
        // Read existing content into a memory stream to ensure it can be read again safely
        var ms = new MemoryStream();
        await request.Content.CopyToAsync(ms).NoCapture();
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
