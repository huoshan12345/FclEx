namespace FclEx.AspNetCore;

public static class HttpRequestExtensions
{
    public static JwtSecurityToken? GetJwtOrNull(this HttpRequest request)
    {
        var tokenStr = request.Headers[HeaderNames.Authorization]
            .ToString()
            .SkipUntil(OidcConstants.TokenResponse.BearerTokenType)
            .Trim();

        return string.IsNullOrEmpty(tokenStr)
            ? null
            : new JwtSecurityToken(tokenStr);
    }

    // Some properties may be null in unit tests
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static IPAddress? RemoteIpAddressOrNull(this HttpRequest request)
    {
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
}