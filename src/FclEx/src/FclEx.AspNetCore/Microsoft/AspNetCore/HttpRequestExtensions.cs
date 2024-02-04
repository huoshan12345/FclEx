using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore;

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

    // To fix null-ref issue in Seismic.Common.ServiceFoundation.RequestExtensions.RemoteIpAddress() 
    // Some properties may be null in unit tests
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