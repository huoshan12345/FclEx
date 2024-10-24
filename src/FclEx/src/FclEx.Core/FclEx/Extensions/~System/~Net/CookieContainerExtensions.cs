namespace FclEx.Extensions;

public static class CookieContainerExtensions
{
    private static readonly FieldInfo FieldOfDomainTable = typeof(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static List<Cookie> GetAllCookies(this CookieContainer cookieJar)
    {
        var list = new List<Cookie>(cookieJar.Count);

        var table = (Hashtable)FieldOfDomainTable.GetValue(cookieJar)!;

        var cookieLists = new List<SortedList>();
        lock (table.SyncRoot)
        {
            foreach (var pathList in table.Values)
            {
                var cookieList = (SortedList)pathList.GetType().InvokeMember("m_list",
                    BindingFlags.NonPublic |
                    BindingFlags.GetField |
                    BindingFlags.Instance,
                    null,
                    pathList,
                    new object[] { })!;

                cookieLists.Add(cookieList);
            }
        }

        foreach (var cookieList in cookieLists)
        {
            lock (cookieList.SyncRoot)
            {
                foreach (CookieCollection cookieCollection in cookieList.Values)
                {
                    foreach (Cookie cookie in cookieCollection)
                    {
                        list.Add(cookie);
                    }
                }
            }
        }

        return list;
    }
}