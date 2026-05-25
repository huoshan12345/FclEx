namespace FclEx.YamlDotNet;

/// <summary>
/// Provides index-based operations for YamlDotNet ordered dictionaries.
/// </summary>
public static class OrderedDictionaryExtensions
{
    /// <summary>
    /// Swaps two entries in an ordered dictionary.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="dictionary">The ordered dictionary to update.</param>
    /// <param name="index1">The zero-based index of the first entry.</param>
    /// <param name="index2">The zero-based index of the second entry.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dictionary"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when either index is outside the dictionary bounds.</exception>
    public static void Swap<TKey, TValue>(this global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int index1, int index2)
        where TKey : notnull
    {
        Check.NotNull(dictionary);
        SwapCore(dictionary, index1, index2);
    }

    /// <summary>
    /// Swaps two entries without repeating null validation.
    /// </summary>
    private static void SwapCore<TKey, TValue>(global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int index1, int index2)
        where TKey : notnull
    {
        if (index1 == index2)
            return;

        var item1 = dictionary[index1];
        var item2 = dictionary[index2];
        dictionary[index1] = item2;
        dictionary[index2] = item1;
    }

    /// <summary>
    /// Moves an entry from one index to another while preserving the relative order of the other entries.
    /// </summary>
    /// <typeparam name="TKey">The dictionary key type.</typeparam>
    /// <typeparam name="TValue">The dictionary value type.</typeparam>
    /// <param name="dictionary">The ordered dictionary to update.</param>
    /// <param name="sourceIndex">The zero-based index of the entry to move.</param>
    /// <param name="destinationIndex">The zero-based index where the entry should be placed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dictionary"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when either index is outside the dictionary bounds.</exception>
    public static void MoveAt<TKey, TValue>(this global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int sourceIndex, int destinationIndex)
        where TKey : notnull
    {
        Check.NotNull(dictionary);
        Check.Between(sourceIndex, 0, dictionary.Count - 1);
        Check.Between(destinationIndex, 0, dictionary.Count - 1);

        if (sourceIndex == destinationIndex)
            return;

        if (sourceIndex < destinationIndex)
        {
            for (var i = sourceIndex; i < destinationIndex; i++)
            {
                SwapCore(dictionary, i, i + 1);
            }
        }
        else
        {
            for (var i = sourceIndex; i > destinationIndex; i--)
            {
                SwapCore(dictionary, i, i - 1);
            }
        }
    }
}
