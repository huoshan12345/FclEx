namespace FclEx.Http;

/// <summary>
/// Extensions for adding HTTP request data to a logger property scope group.
/// </summary>
public static class LoggerPropertyScopeExtensions
{
    /// <summary>
    /// Pushes request path, host, content metadata, and method values from an HTTP request message.
    /// </summary>
    /// <param name="properties">The scope group that owns the request property scopes.</param>
    /// <param name="request">The request whose URI, content headers, and method are captured immediately.</param>
    /// <returns>The same scope group for further pushes.</returns>
    public static LoggerPropertyScope Push(this LoggerPropertyScope properties, HttpRequestMessage request)
    {
        var uri = request.RequestUri;
        properties
            .Push(LogPropertyNames.RequestPath, uri?.GetPath())
            .Push(nameof(Uri.Host), uri?.Host)
            .Push(nameof(HttpContentHeaders.ContentType), request.Content?.Headers.ContentType)
            .Push(nameof(HttpContentHeaders.ContentLength), request.Content?.Headers.ContentLength)
            .Push(nameof(HttpRequestMessage.Method), request.Method);

        return properties;
    }
}
