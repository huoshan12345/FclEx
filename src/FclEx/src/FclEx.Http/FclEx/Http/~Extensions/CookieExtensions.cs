namespace FclEx.Http;

public static class SimpleCookieExtensions
{
    public static SimpleCookie ToSimpleCookie(this Cookie cookie)
    {
        return new SimpleCookie(cookie.Name, cookie.Value, cookie.Domain);
    }
}