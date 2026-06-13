namespace FclEx.Http;

/// <summary>
/// Extensions for inspecting handlers stored inside <see cref="HttpMessageInvoker"/>.
/// </summary>
public static class HttpMessageInvokerExtensions
{
    /// <summary>
    /// Gets the inner handler stored by <see cref="HttpMessageInvoker"/>.
    /// </summary>
    /// <remarks>
    /// This method reads a private runtime field. It is useful for diagnostics and tests, but may break if
    /// <see cref="HttpMessageInvoker"/> internals change and may not be suitable for trimming or AOT scenarios.
    /// </remarks>
    public static T GetHandler<T>(this HttpMessageInvoker invoker) where T : HttpMessageHandler
    {
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<T>(invoker);
    }

    /// <summary>
    /// Gets the inner handler stored by <see cref="HttpMessageInvoker"/>.
    /// </summary>
    /// <remarks>
    /// This method reads a private runtime field. It is useful for diagnostics and tests, but may break if
    /// <see cref="HttpMessageInvoker"/> internals change and may not be suitable for trimming or AOT scenarios.
    /// </remarks>
    public static HttpMessageHandler GetHandler(this HttpMessageInvoker invoker)
    {
        return invoker.GetHandler<HttpMessageHandler>();
    }
}
