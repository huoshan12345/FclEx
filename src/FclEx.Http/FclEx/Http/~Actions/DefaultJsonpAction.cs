namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IJsonpAction{T}"/>.
/// </summary>
public static class DefaultJsonpAction
{
    /// <summary>
    /// Adds the JSONP callback query parameter to a request.
    /// </summary>
    /// <typeparam name="T">The JSONP result type.</typeparam>
    /// <param name="action">The JSONP action.</param>
    /// <param name="request">The request to modify.</param>
    public static void ModifyRequest<T>(IJsonpAction<T> action, HttpRequest request)
    {
        request.AddQueryParam(action.CallbackParamName, Regexes.CallbackName);
    }

    /// <summary>
    /// Extracts JSON text from a JSONP response.
    /// </summary>
    /// <typeparam name="T">The JSONP result type.</typeparam>
    /// <param name="action">The JSONP action.</param>
    /// <param name="response">The response containing callback-wrapped JSON.</param>
    /// <returns>The callback body, or an error when the callback wrapper is missing.</returns>
    public static OperationResult<string> GetJson<T>(IJsonpAction<T> action, HttpResponse response)
    {
        var match = Regexes.CallbackContent.Match(response.ResponseString);
        return match.Success
            ? Operation.Success(match.Value)
            : Operation.Error<string>("Failed to parse callback: " + response.ResponseString.Truncate(200));
    }
}
