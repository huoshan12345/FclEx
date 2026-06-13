namespace FclEx.Http;

/// <summary>
/// Conversion helpers between <see cref="SimpleCookie"/> and <see cref="Cookie"/>.
/// </summary>
public static class SimpleCookieExtensions
{
    /// <summary>
    /// Converts a simple cookie model to <see cref="Cookie"/> using name, value, path, and domain.
    /// </summary>
    public static Cookie ToCookie(this SimpleCookie simpleCookie)
    {
        return new Cookie(simpleCookie.Name, simpleCookie.Value, simpleCookie.Path, simpleCookie.Domain);
    }

    /// <summary>
    /// Converts a <see cref="Cookie"/> to the serializable simple cookie model.
    /// </summary>
    public static SimpleCookie ToSimpleCookie(this Cookie cookie)
    {
        return new SimpleCookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
    }
}
