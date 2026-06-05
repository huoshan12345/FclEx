#if NET5_0_OR_GREATER
namespace FclEx.Http;

public static class HttpRequestOptionsExtensions
{
    public static void Set<TValue>(this HttpRequestOptions options, string key, TValue value)
    {
        options.Set(new HttpRequestOptionsKey<TValue>(key), value);
    }
}
#endif