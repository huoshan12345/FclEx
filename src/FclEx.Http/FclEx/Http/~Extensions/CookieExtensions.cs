namespace FclEx.Http;

/// <summary>
/// Conversion helpers between <see cref="SimpleCookie"/> and <see cref="Cookie"/>.
/// </summary>
public static class CookieExtensions
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

    extension(Cookie)
    {
        /// <summary>
        /// Parses one or more cookies from a header string.
        /// Cookies with empty names and cookies rejected by <see cref="Cookie"/> validation are returned as error results while other cookies continue to be parsed.
        /// </summary>
        public static IEnumerable<OperationResult<Cookie>> Parse(string cookieStr)
        {
            var parser = new CookieParser(cookieStr);

            while (true)
            {
                var cookie = parser.Get();
                if (cookie == null)
                    break;

                if (cookie.Name.IsNullOrEmpty())
                {
                    yield return "A cookie has been ignored due to empty name: " + cookie;
                    continue;
                }

                yield return cookie;
            }
        }
    }
}
