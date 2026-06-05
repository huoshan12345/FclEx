namespace FclEx.Http;

/// <summary>
/// Provides default behavior for <see cref="IXmlAction{T}"/>.
/// </summary>
public static class DefaultXmlAction
{
    /// <summary>
    /// Reads XML, creates a context, and converts the selected element to the result type.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The XML action.</param>
    /// <param name="response">The response containing XML text.</param>
    /// <returns>The converted result, or an error from validation, XPath matching, or deserialization.</returns>
    public static OperationResult<T> GetResult<T>(IXmlAction<T> action, HttpResponse response)
    {
        return action.GetXml(response)
            .Then(m => action.CreateContext(response, m))
            .Then(action.GetResult);
    }

    /// <summary>
    /// Gets XML text from a response.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The XML action.</param>
    /// <param name="response">The response to read.</param>
    /// <returns>The response text when it looks like XML; otherwise an error result.</returns>
    public static OperationResult<string> GetXml<T>(IXmlAction<T> action, HttpResponse response)
    {
        var str = response.ResponseString;
        return str.IsPossibleXml()
            ? Operation.Success(response.ResponseString)
            : Operation.Error<string>("The response string is not a valid xml: " + str.Truncate(256));
    }

    /// <summary>
    /// Creates an XML context and verifies that the configured XPath has a match.
    /// </summary>
    /// <typeparam name="T">The action result type.</typeparam>
    /// <param name="action">The XML action.</param>
    /// <param name="response">The source response.</param>
    /// <param name="xml">The XML text to parse.</param>
    /// <returns>A context when a result element exists; otherwise an error result.</returns>
    /// <remarks>Malformed XML may throw so the outer action pipeline can capture it.</remarks>
    public static OperationResult<XmlActionContext> CreateContext<T>(IXmlAction<T> action, HttpResponse response, string xml)
    {
        var context = new XmlActionContext(response, xml, action.XPath);
        if (context.ResultElements.IsNotEmpty())
            return context;
        const string msg = "The result object does not exist in xml";
        var error = action.XPath == null ? msg : msg + " at " + action.XPath;
        error = error + ": " + context.Xml.Truncate(256);
        return error;
    }

    /// <summary>
    /// Converts the first selected XML element to the result type.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The XML action.</param>
    /// <param name="context">The parsed XML context.</param>
    /// <returns>The deserialized result, or an error if no element was selected.</returns>
    public static OperationResult<T> GetResult<T>(IXmlAction<T> action, XmlActionContext context)
    {
        return context.ResultElement is { } element
            ? element.ToObject<T>()!
            : nameof(context.ResultElement) + " is null";
    }
}