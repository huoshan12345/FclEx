namespace FclEx.Http;

public static class CookieHelper
{
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