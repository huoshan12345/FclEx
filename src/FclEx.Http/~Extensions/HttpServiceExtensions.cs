using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Helpers;
using FclEx.Http.Core;
using FclEx.Http.Services;

namespace FclEx.Http
{
    public static class HttpServiceExtensions
    {
        public static ValueTask<HttpRes> GetAsync(this IHttpService http, string url, string charSet = null, int? timeout = 10 * 1000, int retryTimes = 3, int delaySeconds = 0)
        {
            var req = HttpReq.Get(url)
                .Compress()
                .Timeout(timeout)
                .ChartSet(charSet);
            return SendAsync(http, req, retryTimes, delaySeconds);
        }

        public static async ValueTask<HttpRes> SendAsync(this IHttpService http, HttpReq req, int retryTimes = 1, int delaySeconds = 0)
        {
            return await ActionHelper.TryAsync(async ()
                => await http.ExecuteAsync(req).DonotCapture(),
                retryTimes, delaySeconds, HttpRes.CreateError, false, null)
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
            foreach (var cookie in cookies)
            {
                http.AddCookie(cookie, uri);
            }
        }

        public static void AddCookies(this IHttpService http, IEnumerable<Cookie> cookies, Uri uri)
        {
            foreach (var cookie in cookies)
            {
                http.AddCookie(cookie, uri);
            }
        }

        public static void AddCookies(this IHttpService http, CookieCollection cc, string url = null) => AddCookies(http, cc.OfType<Cookie>(), url);
    }
}
