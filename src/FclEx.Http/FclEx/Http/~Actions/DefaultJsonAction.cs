namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IJsonAction{T}"/>.
/// </summary>
public static class DefaultJsonAction
{
    /// <summary>
    /// Reads JSON, creates a context, and converts the selected token to the result type.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The JSON action.</param>
    /// <param name="response">The response containing JSON text.</param>
    /// <returns>The converted result, or an error from validation, path matching, or deserialization.</returns>
    public static OperationResult<T> GetResult<T>(IJsonAction<T> action, HttpResponse response)
    {
        return action.GetJson(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    /// <summary>
    /// Gets JSON text from a response.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The JSON action.</param>
    /// <param name="response">The response to read.</param>
    /// <returns>The response text when it looks like JSON; otherwise an error result.</returns>
    public static OperationResult<string> GetJson<T>(IJsonAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleJson()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid json: " + str.Truncate(256));
    }

    /// <summary>
    /// Creates a JSON context and verifies that the configured path has a match.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The JSON action.</param>
    /// <param name="response">The source response.</param>
    /// <param name="json">The JSON text to parse.</param>
    /// <returns>A context when a result token exists; otherwise an error result.</returns>
    /// <remarks>Malformed JSON is allowed to throw so the outer action pipeline can capture it.</remarks>
    public static OperationResult<JsonActionContext> CreateContext<T>(IJsonAction<T> action, HttpResponse response, string json)
    {
        var context = new JsonActionContext(response, json, action.JsonPath);
        if (context.ResultTokens.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in json";
        var error = action.JsonPath == null ? msg : msg + " at " + action.JsonPath;
        error = error + ": " + context.Json.Truncate(256);
        return error;
    }

    /// <summary>
    /// Converts the first selected JSON token to the result type.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The JSON action.</param>
    /// <param name="context">The parsed JSON context.</param>
    /// <returns>The deserialized result, or an error if no token was selected.</returns>
    public static OperationResult<T> GetResult<T>(IJsonAction<T> action, JsonActionContext context)
    {
        return context.TryGetResultToken(out var token)
            ? token.ToObject<T>()!
            : nameof(context.ResultToken) + " is null";
    }
}
