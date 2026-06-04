namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IHtmlAction{T}"/>.
/// </summary>
public static class DefaultHtmlAction
{
    /// <summary>
    /// Reads HTML, creates a context, and converts the selected element to the result type.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The HTML action.</param>
    /// <param name="response">The response containing HTML text.</param>
    /// <returns>The converted result, or an error from validation or selector matching.</returns>
    public static OperationResult<T> GetResult<T>(IHtmlAction<T> action, HttpResponse response)
    {
        return action.GetHtml(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    /// <summary>
    /// Gets HTML text from a response.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The HTML action.</param>
    /// <param name="response">The response to read.</param>
    /// <returns>The response text, or an error when it is empty.</returns>
    public static OperationResult<string> GetHtml<T>(IHtmlAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str switch
        {
            _ when str.IsNullOrEmpty() => Operation.Error<string>("The response string is empty"),
            _ when str.IsPossibleHtml() => Operation.Success(response.ResponseString),
            _ => Operation.Error<string>("The response string is not a valid html: " + str.Truncate(256))
        };
    }

    /// <summary>
    /// Creates an HTML context and verifies that the configured selector has a match.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The HTML action.</param>
    /// <param name="response">The source response.</param>
    /// <param name="html">The HTML text to parse.</param>
    /// <returns>A context when a result element exists; otherwise an error result.</returns>
    /// <remarks>Invalid selectors may throw so the outer action pipeline can capture them.</remarks>
    public static OperationResult<HtmlActionContext> CreateContext<T>(IHtmlAction<T> action, HttpResponse response, string html)
    {
        var context = new HtmlActionContext(response, html, action.HtmlSelector);
        if (context.ResultElements.IsNotEmpty())
            return context;

        const string msg = "The result object does not exist in html";
        var error = action.HtmlSelector == null ? msg : msg + " at " + action.HtmlSelector;
        error = error + ": " + context.Html.Truncate(256);
        return error;
    }
}
