namespace FclEx.Http;

public static class CookieHelper
{
    public static List<OperationResult<Cookie>> Parse(string cookieStr)
    {
        var list = new List<OperationResult<Cookie>>();
        var parser = new CookieParser(cookieStr);

        while (true)
        {
            var c = parser.Get();
            if (c == null)
                break;

            if (c.Name.IsNullOrEmpty())
            {
                list.Add("A cookie has been ignored due to empty name: " + c);
                continue;
            }

            try
            {
                var cookie = c.ToCookie();
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