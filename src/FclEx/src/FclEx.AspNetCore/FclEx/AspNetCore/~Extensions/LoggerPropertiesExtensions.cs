using FclEx.Logging;

namespace FclEx.AspNetCore;

public static class LoggerPropertiesExtensions
{
    public static LoggerProperties Push(this LoggerProperties properties, HttpRequest request)
    {
        var ip = request.RemoteIpAddressOrNull();

        properties
            .Push(nameof(ConnectionInfo.RemoteIpAddress), ip)
            .Push(LogPropertyNames.Path, request.Path)
            .Push(nameof(HttpRequest.ContentType), request.ContentType)
            .Push(nameof(HttpRequest.ContentLength), request.ContentLength)
            .Push(nameof(HttpRequest.Host), request.Host)
            .Push(nameof(HttpRequest.Protocol), request.Protocol)
            .Push(nameof(HttpRequest.Method), request.Method);

        return properties;
    }
}