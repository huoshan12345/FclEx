namespace FclEx.Http;

/// <summary>
/// Extensions for adding HTTP request data to structured logger properties.
/// </summary>
public static class LoggerPropertiesExtensions
{
    /// <summary>
    /// Pushes request path, host, content metadata, and method values from an HTTP request message.
    /// </summary>
    public static LoggerProperties Push(this LoggerProperties properties, HttpRequestMessage request)
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
