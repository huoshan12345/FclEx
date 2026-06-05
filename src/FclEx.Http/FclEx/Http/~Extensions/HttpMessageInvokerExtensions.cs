namespace FclEx.Http;

public static class HttpMessageInvokerExtensions
{
    public static T GetHandler<T>(this HttpMessageInvoker invoker) where T : HttpMessageHandler
    {
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<T>(invoker);
    }

    public static HttpMessageHandler GetHandler(this HttpMessageInvoker invoker)
    {
        return invoker.GetHandler<HttpMessageHandler>();
    }
}