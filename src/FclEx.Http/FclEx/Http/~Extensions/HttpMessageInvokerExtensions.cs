namespace FclEx.Http;

/// <summary>
/// Extensions for inspecting and replacing handlers stored inside <see cref="HttpMessageInvoker"/>.
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
    /// <typeparam name="T">The expected root handler type; the handler chain is not searched.</typeparam>
    /// <param name="invoker">The invoker or HttpClient to inspect.</param>
    /// <returns>The existing root handler converted to the requested type.</returns>
    /// <exception cref="InvalidCastException">The root handler cannot be converted to the requested type.</exception>
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
    /// <param name="invoker">The invoker or HttpClient to inspect.</param>
    /// <returns>The existing root handler, including any outer delegating handler.</returns>
    public static HttpMessageHandler GetHandler(this HttpMessageInvoker invoker)
    {
#if NET8_0_OR_GREATER
        return UnsafeAccessors.HttpMessageInvoker_Handler(invoker);
#else
        return FieldInfos.HttpMessageInvoker_Handler.GetRequiredValue<HttpMessageHandler>(invoker);
#endif
    }

    /// <summary>Replaces an invoker's root handler in place without disposing the previous handler.</summary>
    /// <typeparam name="T">The invoker type, preserved for fluent use with HttpClient and other subclasses.</typeparam>
    /// <param name="invoker">The invoker to modify while no sends or disposal are in progress.</param>
    /// <param name="handler">The replacement root handler; its chain must already be configured.</param>
    /// <returns>The same invoker.</returns>
    /// <remarks>
    /// The original disposeHandler constructor setting applies to the replacement. The caller remains responsible
    /// for releasing the previous handler. This method does not reset HttpClient's started or disposed state and
    /// does not validate arguments. It accesses a private runtime field and depends on its name and type.
    /// </remarks>
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
