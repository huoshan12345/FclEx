namespace FclEx.Http;

/// <summary>
/// Converts an HTTP response into an operation result.
/// </summary>
/// <typeparam name="T">The result type produced from the response.</typeparam>
public interface IHttpResponseHandler<T>
{
    /// <summary>
    /// Gets the result synchronously from a response.
    /// </summary>
    /// <param name="response">The response to convert.</param>
    /// <returns>The converted operation result.</returns>
    OperationResult<T> GetResult(HttpResponse response);

    /// <summary>
    /// Gets the result asynchronously from a response.
    /// </summary>
    /// <param name="response">The response to convert.</param>
    /// <returns>The converted operation result.</returns>
    Task<OperationResult<T>> GetResultAsync(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
#else
    ;
#endif
}

/// <summary>
/// Provides default behavior for <see cref="IHttpResponseHandler{T}"/>.
/// </summary>
public static class DefaultHttpResponseHandler
{
    /// <summary>
    /// Wraps the synchronous response conversion in a completed task.
    /// </summary>
    /// <typeparam name="T">The response result type.</typeparam>
    /// <param name="handler">The response handler.</param>
    /// <param name="response">The response to convert.</param>
    /// <returns>A completed task containing the converted result.</returns>
    public static Task<OperationResult<T>> GetResultAsync<T>(IHttpResponseHandler<T> handler, HttpResponse response)
    {
        return handler.GetResult(response);
    }
}

/// <summary>
/// Base class for response handlers.
/// </summary>
/// <typeparam name="T">The result type produced from the response.</typeparam>
public abstract class HttpResponseHandler<T> : IHttpResponseHandler<T>
{
    /// <inheritdoc />
    public abstract OperationResult<T> GetResult(HttpResponse response);

    /// <inheritdoc />
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
}
