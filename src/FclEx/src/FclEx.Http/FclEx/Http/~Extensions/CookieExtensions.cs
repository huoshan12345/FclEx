using System.Net;
using FclEx.Http.Core.Cookies;

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
