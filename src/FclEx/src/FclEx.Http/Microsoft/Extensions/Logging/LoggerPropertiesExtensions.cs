using FclEx.Logging;

namespace Microsoft.Extensions.Logging;

public static class LoggerPropertiesExtensions
{
    public static LoggerProperties Push(this LoggerProperties properties, HttpRequestMessage request)
    {
        var uri = request.RequestUri;
        properties
            .Push(LogPropertyNames.Path, uri?.LocalPath)
            .Push(nameof(HttpContentHeaders.ContentType), request.Content?.Headers.ContentType)
            .Push(nameof(HttpContentHeaders.ContentLength), request.Content?.Headers.ContentLength)
            .Push(nameof(Uri.Host), uri?.Host)
            .Push(nameof(HttpRequestMessage.Method), request.Method);

        return properties;
    }
}