namespace FclEx.Http;

public static class SimpleCookieExtensions
{
    public static Cookie ToCookie(this SimpleCookie simpleCookie)
    {
        return new Cookie(simpleCookie.Name, simpleCookie.Value, simpleCookie.Path, simpleCookie.Domain);
    }

    public static SimpleCookie ToSimpleCookie(this Cookie cookie)
    {
        return new SimpleCookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
    }
}