namespace FclEx.Extensions;

public static class CookieCollectionExtensions
{
    public static IEnumerable<Cookie> Enumerate(this CookieCollection collection)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        return collection as IEnumerable<Cookie> ?? collection.Cast<Cookie>();
    }
}