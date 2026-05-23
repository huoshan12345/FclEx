namespace FclEx.Extensions;

public static partial class DictionaryExtensions
{
    public static IEnumerable<KeyValuePair<TKey, TValue?>> Enumerate<TKey, TValue>(this IDictionary dic)
    {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (DictionaryEntry entry in dic)
        {
            yield return new((TKey)entry.Key, (TValue?)entry.Value);
        }
    }

    public static IEnumerable<KeyValuePair<string, string?>> EnumerateStringPairs(this IDictionary dic)
    {
        return dic.Enumerate<string, string?>();
    }
}
