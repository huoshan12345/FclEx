using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Core.Cookies;
using FclEx.Utils;

namespace FclEx.Http.Services
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static class HttpServiceExtensions
    {
        public static Task<HttpRes> GetAsync(this IHttpService http, string url, string charSet = null, int? timeout = 10 * 1000, int retryTimes = 3, int delaySeconds = 0)
        {
            var req = HttpReq.Get(url)
                .Compress()
                .Timeout(timeout)
                .CharSet(charSet);
            return SendAsync(http, req, retryTimes, delaySeconds);
        }

        public static async Task<HttpRes> SendAsync(this IHttpService http, HttpReq req, int retryTimes = 1, int delaySeconds = 0)
        {
            return await ActionHelper.TryAsync(async ()
                => await http.ExecuteAsync(req).DonotCapture(),
                retryTimes, delaySeconds, e => HttpRes.CreateError(req, e), false, null)
                .DonotCapture();
        }

        public static void AddCookie(this IHttpService http, Cookie cookie, string url = null)
        {
            var uri = url == null ? null : new Uri(url);
            http.AddCookie(cookie, uri);
        }

        public static Cookie GetCookie(this IHttpService http, string url, string name)
        {
            var uri = new Uri(url);
            return http.GetCookie(uri, name);
        }

        public static IReadOnlyList<Cookie> GetCookies(this IHttpService http, string url)
        {
            var uri = new Uri(url);
            return http.GetCookies(uri);
        }

        public static void ClearCookies(this IHttpService http, Uri uri)
        {
            foreach (var cookie in http.GetCookies(uri))
            {
                cookie.Expired = true;
            }
        }

        public static void ClearCookies(this IHttpService http, string url)
        {
            var uri = new Uri(url);
            http.ClearCookies(uri);
        }

        public static void ClearAllCookies(this IHttpService http)
        {
            foreach (var cookie in http.GetAllCookies())
            {
                cookie.Expired = true;
            }
        }

        public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, string url = null)
        {
            var uri = url == null ? null : new Uri(url);
            http.AddCookies(cookies, uri);
        }

        public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, Uri uri)
        {
            Guard.Argument(http, nameof(http)).NotNull();
            Guard.Argument(cookies, nameof(cookies)).NotNull();
            foreach (var cookie in cookies)
                http.AddCookie(cookie, uri);
        }

        public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, Uri uri)
            => http.AddCookies(cookies.Select(m => m.ToCookie()), uri);

        public static void AddCookies(this IHttpService http, IEnumerable<SimpleCookie> cookies, string url = null)
        {
            var uri = url == null ? null : new Uri(url);
            http.AddCookies(cookies, uri);
        }

        public static IReadOnlyList<SimpleCookie> GetAllSimpleCookies(this IHttpService http)
        {
            Guard.Argument(http, nameof(http)).NotNull();
            return http.GetAllCookies().Select(m => m.ToSimpleCookie()).ToList();
        }

        public static void AddCookie(this IHttpService http, SimpleCookie cookie)
        {
            Guard.Argument(http, nameof(http)).NotNull();
            Guard.Argument(cookie, nameof(cookie)).NotNull();
            http.AddCookie(cookie.ToCookie());
        }

        public static void AddCookies(this IHttpService http, CookieCollection cc, string url = null)
            => AddCookies(http, cc.OfType<Cookie>(), url);

        public static async Task<OperateResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, Uri uri,
            HttpMethodType method = HttpMethodType.Get, TimeSpan? timeout = null)
        {
            var req = new HttpReq(uri, method)
                .ResultType(HttpResultType.Byte)
                .Timeout(timeout)
                .Compress();

            var res = await http.SendAsync(req).DonotCapture();
            if (res.HasError)
                return OperateResult.CreateObjError(res, res.Exception, res.ExcuteTime);
            else
                return res.GetDownloadInfo();
        }

        public static Task<OperateResult<HttpFileDownloadInfo>> DownloadAsync(this IHttpService http, string url,
            HttpMethodType method = HttpMethodType.Get, TimeSpan? timeout = null)
            => http.DownloadAsync(new Uri(url), method, timeout);
    }
}
