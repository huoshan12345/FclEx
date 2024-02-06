namespace Microsoft.AspNetCore.Http;

public static class SessionExtensions
{
    public static bool TryGetString(this ISession session, string key, [NotNullWhen(true)] out string? value, Encoding? encoding = null)
    {
        if (session.TryGetValue(key, out var bytes))
        {
            value = bytes.GetString(encoding);
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }

    public static bool TryPopString(this ISession session, string key, out string? value)
    {
        if (session.TryGetString(key, out value))
        {
            session.Remove(key);
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}