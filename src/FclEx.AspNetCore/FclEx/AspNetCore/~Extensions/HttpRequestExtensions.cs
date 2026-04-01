namespace FclEx.AspNetCore;

public static class HttpRequestExtensions
{
    public static JsonWebToken? GetJwtOrNull(this HttpRequest request)
    {
        var tokenStr = request.Headers[HeaderNames.Authorization]
            .ToString()
            .SkipUntil(OidcConstants.TokenResponse.BearerTokenType)
            .Trim();

        return string.IsNullOrEmpty(tokenStr)
            ? null
            : new JsonWebToken(tokenStr);
    }

    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static IPAddress? RemoteIpAddressOrNull(this HttpRequest request)
    {
        // ReSharper disable once InvertIf
        if (request.Headers?.TryGetValue("X-Real-IP", out var header) is true)
        {
            var couldParse = IPAddress.TryParse(header, out var address);
            if (couldParse)
            {
                return address;
            }
        }

        return request.HttpContext?.Connection?.RemoteIpAddress;
    }

    public static async Task<string> GetRawBodyAsync(this HttpRequest request, Encoding? encoding = null)
    {
        if (request.Body.CanSeek == false)
        {
            // We only do this if the stream isn't *already* seekable,
            // as EnableBuffering will create a new stream instance
            // each time it's called
            request.EnableBuffering();
        }

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }
    
    public static JwtInfo? GetJwtInfo(this HttpRequest request)
    {
        var items = request.HttpContext.Items;

        if (items.TryGetValue(nameof(JwtInfo), out var value) && value is JwtInfo info)
            return info;

        var token = request.GetJwtOrNull();
        if (token is null)
            return null;

        info = new JwtInfo(token);
        items[nameof(JwtInfo)] = info;
        return info;
    }

    /// <summary>
    /// Retrieves a parameter value by checking HTTP headers, query string, and cookies in that order.
    /// Returns the first non-null value found or null if no value exists.
    /// </summary>
    /// <param name="request">The HTTP request to extract the parameter from</param>
    /// <param name="headerKey">The key to look up in the request headers</param>
    /// <param name="queryKey">Optional key to look up in query string. Defaults to headerKey if not specified</param>
    /// <param name="cookieKey">Optional key to look up in cookies. Defaults to headerKey if not specified</param>
    /// <returns>The parameter value found or null if not found in any location</returns>
    public static string? GetParameterValue(this HttpRequest request, string headerKey, string? queryKey = null, string? cookieKey = null)
    {
        queryKey ??= headerKey;
        cookieKey ??= headerKey;
        return request.Headers[headerKey].LastOrDefault()
               ?? request.Query[queryKey].LastOrDefault()
               ?? request.Cookies[cookieKey];
    }
}