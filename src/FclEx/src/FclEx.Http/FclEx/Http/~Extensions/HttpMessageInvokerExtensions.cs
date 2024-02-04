namespace FclEx.Http;

public static class HttpMessageInvokerExtensions
{
    private static readonly FieldInfo _handler = typeof(HttpMessageInvoker).GetRequiredField("_handler");

    public static T GetHandler<T>(this HttpMessageInvoker invoker) where T : HttpMessageHandler
    {
        return _handler.GetRequiredValue<T>(invoker);
    }

    public static HttpMessageHandler GetHandler(this HttpMessageInvoker invoker)
    {
        return invoker.GetHandler<HttpMessageHandler>();
    }
}