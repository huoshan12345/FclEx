using System.Net;
using FclEx.Http.Core;
using FclEx.Http.Core.Cookies;
using FclEx.Http.Services;

namespace FclEx.Http
{
    public static class SimpleCookieExtensions
    {
        public static SimpleCookie ToSimpleCookie(this Cookie cookie)
        {
            return new SimpleCookie(cookie.Name, cookie.Value, cookie.Domain);
        }
    }
}
