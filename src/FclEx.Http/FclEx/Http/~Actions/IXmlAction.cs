namespace FclEx.Http;

/// <summary>
/// Handles an HTTP response whose body is XML.
/// </summary>
/// <typeparam name="T">The result type produced from the selected XML element.</typeparam>
public interface IXmlAction<T> : IHttpResponseHandler<T>
{
    /// <summary>
    /// Gets the optional XPath used to select result elements.
    /// </summary>
    /// <remarks>When <see langword="null"/>, the document root element is used.</remarks>
    string? XPath
#if NET6_0_OR_GREATER
        => null;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    OperationResult<T> IHttpResponseHandler<T>.GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);
#endif

    /// <summary>
    /// Gets XML text from the response.
    /// </summary>
    /// <param name="response">The response containing XML text.</param>
    /// <returns>The XML text, or an error when the response does not look like XML.</returns>
    OperationResult<string> GetXml(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.GetXml(this, response);
#else
    ;
#endif

    /// <summary>
    /// Creates an XML action context from response XML.
    /// </summary>
    /// <param name="response">The source response.</param>
    /// <param name="xml">The XML text to parse.</param>
    /// <returns>A context when the XPath matches at least one element; otherwise an error result.</returns>
    /// <remarks>Malformed XML may throw; callers that need operation errors should invoke this through the action pipeline.</remarks>
    OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.CreateContext(this, response, xml);
#else
    ;
#endif

    /// <summary>
    /// Converts an XML context into the final result.
    /// </summary>
    /// <param name="context">The parsed XML context.</param>
    /// <returns>The result produced from the selected XML element.</returns>
    OperationResult<T> GetResult(XmlActionContext context)
#if NET6_0_OR_GREATER
        => DefaultXmlAction.GetResult(this, context);
#else
    ;
#endif
}

/// <summary>
/// Handles an XML response when only success or failure matters.
/// </summary>
public interface IXmlAction : IXmlAction<Unit>
{
#if NET6_0_OR_GREATER
    /// <inheritdoc />
    OperationResult IXmlAction<Unit>.GetResult(XmlActionContext context) => Operation.Success();
#endif
}
