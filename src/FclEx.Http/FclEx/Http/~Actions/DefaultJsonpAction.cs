namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IJsonpAction{T}"/>.
/// </summary>
public static class DefaultJsonpAction
{
    /// <summary>
    /// The default JSONP callback function name sent with requests and expected in responses.
    /// </summary>
    public const string DefaultCallbackName = "_callback";

    /// <summary>
    /// Adds the JSONP callback query parameter to a request.
    /// </summary>
    /// <typeparam name="T">The JSONP result type.</typeparam>
    /// <param name="action">The JSONP action.</param>
    /// <param name="request">The request to modify.</param>
    public static void ModifyRequest<T>(IJsonpAction<T> action, HttpRequest request)
    {
        request.AddQueryParam(action.CallbackParamName, action.CallbackName);
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
        return TryGetCallbackBody(response.ResponseString, action.CallbackName, out var json)
            ? Operation.Success(json)
            : Operation.Error<string>("Failed to parse JSONP callback: " + response.ResponseString.Truncate(200));
    }

    private static bool TryGetCallbackBody(
        string responseString,
        string callbackName,
        [NotNullWhen(true)] out string? json)
    {
        json = null;

        if (responseString.IsNullOrWhiteSpace() || callbackName.IsNullOrWhiteSpace())
        {
            return false;
        }

        var text = responseString.Trim();
        if (text.EndsWith(';'))
        {
            text = text[..^1].TrimEnd();
        }

        if (!text.StartsWith(callbackName, StringComparison.Ordinal))
        {
            return false;
        }

        var start = callbackName.Length;
        while (start < text.Length && char.IsWhiteSpace(text[start]))
        {
            start++;
        }

        if (start >= text.Length || text[start] != '(')
        {
            return false;
        }

        var end = text.Length - 1;
        while (end > start && char.IsWhiteSpace(text[end]))
        {
            end--;
        }

        if (end <= start || text[end] != ')')
        {
            return false;
        }

        json = text.Substring(start + 1, end - start - 1).Trim();
        return json.IsNotEmpty();
    }
}
