namespace FclEx.Extensions;

public static class CookieContainerExtensions
{
#if NETSTANDARD2_0
    private static readonly FieldInfo _domainTable = typeof(CookieContainer).GetRequiredField("m_domainTable");

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static CookieCollection GetAllCookies(this CookieContainer container)
    {
        var result = new CookieCollection();
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
                        result.Add(cookieCollection);
                    }
                }
            }
        }
        return result;
    }
#endif
}