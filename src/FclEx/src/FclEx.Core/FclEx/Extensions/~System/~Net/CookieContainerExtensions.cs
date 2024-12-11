namespace FclEx.Extensions;

public static class CookieContainerExtensions
{
    private static readonly FieldInfo _domainTable = typeof(CookieContainer).GetRequiredField("m_domainTable");

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static List<Cookie> GetAllCookies(this CookieContainer container)
    {
        var list = new List<Cookie>(container.Count);
        var table = _domainTable.GetRequiredValue<Hashtable>(container);

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
                    [])!;

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
        }
        return list;
    }
}