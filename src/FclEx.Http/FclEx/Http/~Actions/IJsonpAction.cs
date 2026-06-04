namespace FclEx.Http;

/// <summary>
/// Represents an HTTP JSONP action.
/// </summary>
/// <typeparam name="T">The result type produced from the JSONP payload.</typeparam>
public interface IJsonpAction<T> : IJsonAction<T>, IHttpAction<T>
{
    /// <summary>
    /// Gets the query parameter name used for the JSONP callback.
    /// </summary>
    string CallbackParamName { get; }

#if NET6_0_OR_GREATER
    /// <inheritdoc />
    string? IJsonAction<T>.JsonPath => null;

    /// <inheritdoc />
    HttpMethod IHttpAction<T>.Method => HttpMethod.Get;

    /// <inheritdoc />
    void IHttpAction<T>.ModifyRequest(HttpRequest request) 
        => DefaultJsonpAction.ModifyRequest(this, request);

    /// <inheritdoc />
    OperationResult<string> IJsonAction<T>.GetJson(HttpResponse response)
        => DefaultJsonpAction.GetJson(this, response);
#endif
}
