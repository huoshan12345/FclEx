namespace FclEx.Http;

/// <summary>
/// Helpers for parsing Set-Cookie style header text into <see cref="Cookie"/> values.
/// </summary>
public static class CookieHelper
{
    /// <summary>
    /// Parses one or more cookies from a header string.
    /// Cookies with empty names and cookies rejected by <see cref="Cookie"/> validation are returned as error results while other cookies continue to be parsed.
    /// </summary>
    public static List<OperationResult<Cookie>> Parse(string cookieStr)
    {
        var list = new List<OperationResult<Cookie>>();
        var parser = new CookieParser(cookieStr);

        while (true)
        {
            var cookie = parser.Get();
            if (cookie == null)
                break;

            if (cookie.Name.IsNullOrEmpty())
            {
                list.Add("A cookie has been ignored due to empty name: " + cookie);
                continue;
            }

            try
            {
                list.Add(cookie);
            }
            catch (Exception ex)
            {
                list.Add(ex);
            }
        }

        return list;
    }
}
