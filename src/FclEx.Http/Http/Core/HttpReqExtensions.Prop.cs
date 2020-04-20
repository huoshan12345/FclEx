using System;
using FclEx.Helpers;

namespace FclEx.Http.Core
{
    public static partial class HttpReqExtensions
    {
        public static HttpReq ReadResultCookie(this HttpReq req, bool read)
        {
            req.ReadResultCookie = read;
            return req;
        }

        public static HttpReq ReadResultHeader(this HttpReq req, bool read)
        {
            req.ReadResultHeader = read;
            return req;
        }

        public static HttpReq ReadResultContent(this HttpReq req, bool read)
        {
            req.ReadResultContent = read;
            return req;
        }

        public static HttpReq ThrowOnNonSuccessCode(this HttpReq req, bool ifThrow)
        {
            req.ThrowOnNonSuccessCode = ifThrow;
            return req;
        }

        public static HttpReq Host(this HttpReq req, string? host)
        {
            req.Host = host;
            return req;
        }

        public static HttpReq Port(this HttpReq req, int port)
        {
            req.Port = port;
            return req;
        }

        public static HttpReq Fragment(this HttpReq req, string? fragment)
        {
            req.Fragment = fragment;
            return req;
        }

        public static HttpReq UserName(this HttpReq req, string? userName)
        {
            req.UserName = userName;
            return req;
        }

        public static HttpReq Password(this HttpReq req, string? password)
        {
            req.Password = password;
            return req;
        }

        public static HttpReq Path(this HttpReq req, string? path)
        {
            req.Path = path;
            return req;
        }

        public static HttpReq Scheme(this HttpReq req, string? scheme)
        {
            req.Scheme = scheme;
            return req;
        }

        public static HttpReq Method(this HttpReq req, HttpMethodType method)
        {
            req.Method = method;
            return req;
        }

        public static HttpReq Method(this HttpReq req, string method)
        {
            return req.Method(method.ToEnum<HttpMethodType>());
        }

        public static HttpReq Auth(this HttpReq req, string? auth)
        {
            return req.AddHeader(HttpKnownHeaderNames.Authorization, auth);
        }

        public static HttpReq BasicAuth(this HttpReq req, string? userName, string? password)
        {
            var userInfo = userName + ":" + password;
            return req.AddHeader(HttpKnownHeaderNames.Authorization, "Basic " + userInfo.ToBytes().ToBase64());
        }

        public static HttpReq BearerAuth(this HttpReq req, string token)
        {
            return req.AddHeader(HttpKnownHeaderNames.Authorization, "Bearer " + token);
        }

        public static HttpReq CharSet(this HttpReq req, string? chartSet)
        {
            req.CharSet = chartSet;
            return req;
        }

        public static HttpReq TryCharSet(this HttpReq req, string? chartSet)
        {
            req.CharSet ??= chartSet;
            return req;
        }

        public static HttpReq DetectCharSetFromHtmlMeta(this HttpReq req, bool flag = true)
        {
            req.DetectCharSetFromHtmlMeta = flag;
            return req;
        }

        public static HttpReq FallbackCharSet(this HttpReq req, string? chartSet)
        {
            req.FallbackCharSet = chartSet;
            return req;
        }

        public static HttpReq TryFallbackCharSet(this HttpReq req, string? chartSet)
        {
            req.FallbackCharSet ??= chartSet;
            return req;
        }

        public static HttpReq Timeout(this HttpReq req, int? timeout)
        {
            return req.Timeout(timeout.HasValue ? TimeSpan.FromMilliseconds(timeout.Value) : (TimeSpan?)null);
        }

        public static HttpReq Timeout(this HttpReq req, TimeSpan? timeout)
        {
            req.Timeout = timeout;
            return req;
        }

        public static HttpReq TryTimeout(this HttpReq req, TimeSpan? timeout)
        {
            req.Timeout ??= timeout;
            return req;
        }

        public static HttpReq TotalTimeout(this HttpReq req, TimeSpan? timeout)
        {
            req.TotalTimeout = timeout;
            return req;
        }

        public static HttpReq TryTotalTimeout(this HttpReq req, TimeSpan? timeout)
        {
            req.TotalTimeout ??= timeout;
            return req;
        }

        public static HttpReq Origin(this HttpReq req, string? url)
        {
            req.Origin = url;
            return req;
        }

        public static HttpReq TryOrigin(this HttpReq req, string? url)
        {
            req.Origin ??= url;
            return req;
        }
    }
}
