namespace FclEx.Http;

public static class NameValueCollectionExtensions
{
    public static bool IsEmpty(this NameValueCollection col)
    {
        return col.Count == 0;
    }
}