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
}