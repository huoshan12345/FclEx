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

/// <summary>
/// Base class for handling an XML response without sending the request itself.
/// </summary>
/// <typeparam name="T">The result type produced from the selected XML element.</typeparam>
public abstract class XmlAction<T> : HttpResponseHandler<T>, IXmlAction<T>
{
    /// <inheritdoc />
    public virtual string? XPath => null;

    /// <inheritdoc />
    public virtual OperationResult<string> GetXml(HttpResponse response)
        => DefaultXmlAction.GetXml(this, response);

    /// <inheritdoc />
    public virtual OperationResult<XmlActionContext> CreateContext(HttpResponse response, string xml)
        => DefaultXmlAction.CreateContext(this, response, xml);

    /// <inheritdoc />
    public virtual OperationResult<T> GetResult(XmlActionContext context)
        => DefaultXmlAction.GetResult(this, context);

    /// <inheritdoc />
    public override OperationResult<T> GetResult(HttpResponse response)
        => DefaultXmlAction.GetResult(this, response);
}

/// <summary>
/// Base class for XML response handlers that only need success or failure.
/// </summary>
public abstract class XmlAction : XmlAction<Unit>, IXmlAction
{
    /// <inheritdoc />
    public override OperationResult GetResult(XmlActionContext context) => Operation.Success();
}
