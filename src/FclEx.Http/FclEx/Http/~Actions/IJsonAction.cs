namespace FclEx.Http;

/// <summary>
/// Handles an HTTP response whose body is JSON.
/// </summary>
/// <typeparam name="T">The result type produced from the selected JSON token.</typeparam>
public interface IJsonAction<T> : IHttpResponseHandler<T>
{
    /// <summary>
    /// Gets the optional JSON path used to select result tokens.
    /// </summary>
    /// <remarks>When <see langword="null"/>, the root JSON token is used.</remarks>
    string? JsonResultPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
        => DefaultJsonAction.GetResult(this, response);
#endif

    /// <summary>
    /// Gets JSON text from the response.
    /// </summary>
    /// <param name="response">The response containing JSON text.</param>
    /// <returns>The JSON text, or an error when the response does not look like JSON.</returns>
    OperationResult<string> GetJson(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.GetJson(this, response);
#else
    ;
#endif

    /// <summary>
    /// Creates a JSON action context from response JSON.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="json">The JSON text to parse.</param>
    /// <returns>A context when the path matches at least one token; otherwise an error result.</returns>
    /// <remarks>Invalid JSON may throw; callers that need operation errors should invoke this through the action pipeline.</remarks>
    OperationResult<JsonActionContext> CreateContext(HttpResponse response, string json)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.CreateContext(this, response, json);
#else
    ;
#endif

    /// <summary>
    /// Converts a JSON context into the final result.
    /// </summary>
    /// <param name="context">The parsed JSON context. The default pipeline disposes it after this method returns.</param>
    /// <returns>The result produced from the selected JSON token.</returns>
    OperationResult<T> GetResult(JsonActionContext context)
#if NET6_0_OR_GREATER
        => DefaultJsonAction.GetResult(this, context);
#else
    ;
#endif
}

/// <summary>
/// Handles a JSON response when only success or failure matters.
/// </summary>
public interface IJsonAction : IJsonAction<Unit>
{
#if NET6_0_OR_GREATER
    /// <inheritdoc />
    OperationResult IJsonAction<Unit>.GetResult(JsonActionContext context) => Operation.Success();
#endif
}
