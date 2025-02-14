namespace FclEx.Extensions;

public static class NameValueCollectionExtensions
{
    public static IEnumerable<KeyValuePair<string, string>> Enumerate(this NameValueCollection collection)
    {
        var q = from k in collection.AllKeys
                from v in collection.GetValues(k).EmptyIfNull()
                select KeyValuePair.Create(k ?? "", v ?? "");
        return q;
    }
}