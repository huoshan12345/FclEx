#if NET5_0_OR_GREATER
namespace FclEx.Http;

/// <summary>
/// Convenience extensions for <see cref="HttpRequestOptions"/>.
/// </summary>
public static class HttpRequestOptionsExtensions
{
    /// <summary>
    /// Sets an option value by constructing a typed <see cref="HttpRequestOptionsKey{TValue}"/> from a string key.
    /// </summary>
    public static void Set<TValue>(this HttpRequestOptions options, string key, TValue value)
    {
        options.Set(new HttpRequestOptionsKey<TValue>(key), value);
    }
}
#endif
