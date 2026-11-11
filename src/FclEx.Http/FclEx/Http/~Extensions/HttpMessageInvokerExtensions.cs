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
        return invoker.GetHandler().CastTo<T>();
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
#if NET8_0_OR_GREATER
        return UnsafeAccessors.HttpMessageInvoker_Handler(invoker);
#else
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<HttpMessageHandler>(invoker);
#endif
    }

    public static T SetHandler<T>(this T invoker, HttpMessageHandler handler) where T : HttpMessageInvoker
    {
#if NET8_0_OR_GREATER
        ref var handlerRef = ref UnsafeAccessors.HttpMessageInvoker_Handler(invoker);
        handlerRef = handler;
#else
        FieldInfos.HttpMessageInvoker_Handler.SetValue(invoker, handler);
#endif
        return invoker;
    }
}
