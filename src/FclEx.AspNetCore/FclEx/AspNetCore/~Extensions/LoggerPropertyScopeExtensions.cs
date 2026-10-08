namespace FclEx.AspNetCore;

/// <summary>Adds ASP.NET Core request metadata to logger property scope groups.</summary>
public static class LoggerPropertyScopeExtensions
{
    /// <summary>Pushes the request path, host, remote address, content metadata, protocol, and method.</summary>
    /// <param name="properties">The scope group that owns the request property scopes.</param>
    /// <param name="request">The request whose current metadata is captured.</param>
    /// <returns>The same scope group for further pushes.</returns>
    public static LoggerPropertyScope Push(this LoggerPropertyScope properties, HttpRequest request)
    {
        var ip = request.RemoteIpAddressOrNull();

        properties
            .Push(LogPropertyNames.RequestPath, request.Path)
            .Push(nameof(HttpRequest.Host), request.Host)
            .Push(nameof(ConnectionInfo.RemoteIpAddress), ip)
            .Push(nameof(HttpRequest.ContentType), request.ContentType)
            .Push(nameof(HttpRequest.ContentLength), request.ContentLength)
            .Push(nameof(HttpRequest.Protocol), request.Protocol)
            .Push(nameof(HttpRequest.Method), request.Method);

        return properties;
    }
}
