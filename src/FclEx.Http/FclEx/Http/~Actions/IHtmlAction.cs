namespace FclEx.Http;

/// <summary>
/// Handles an HTTP response whose body is HTML.
/// </summary>
/// <typeparam name="T">The result type produced from the selected HTML element.</typeparam>
public interface IHtmlAction<T> : IHttpResponseHandler<T>
{
    /// <summary>
    /// Gets the optional CSS selector used to select result elements.
    /// </summary>
    /// <remarks>When <see langword="null"/>, the document element is used.</remarks>
    string? HtmlResultPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

    /// <summary>
    /// Converts an HTML context into the final result.
    /// </summary>
    /// <param name="context">The parsed HTML context.</param>
    /// <returns>The result produced from the selected HTML elements.</returns>
    OperationResult<T> GetResult(HtmlActionContext context);

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
        => DefaultHtmlAction.GetResult(this, response);
#endif

    /// <summary>
    /// Gets HTML text from the response.
    /// </summary>
    /// <param name="response">The response containing HTML text.</param>
    /// <returns>The HTML text, or an error when the response is empty.</returns>
    OperationResult<string> GetHtml(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHtmlAction.GetHtml(this, response);
#else
    ;
#endif

    /// <summary>
    /// Creates an HTML action context from response HTML.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="html">The HTML text to parse.</param>
    /// <returns>A context when the selector matches at least one element; otherwise an error result.</returns>
    /// <remarks>Invalid selectors may throw; callers that need operation errors should invoke this through the action pipeline.</remarks>
    OperationResult<HtmlActionContext> CreateContext(HttpResponse response, string html)
#if NET6_0_OR_GREATER
        => DefaultHtmlAction.CreateContext(this, response, html);
#else
    ;
#endif
}

/// <summary>
/// Handles an HTML response when only success or failure matters.
/// </summary>
public interface IHtmlAction : IHtmlAction<Unit>
{
#if NET6_0_OR_GREATER
    /// <inheritdoc />
    OperationResult IHtmlAction<Unit>.GetResult(HtmlActionContext context) => Operation.Success();
#endif
}
