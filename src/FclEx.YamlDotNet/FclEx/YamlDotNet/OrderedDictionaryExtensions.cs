namespace FclEx.YamlDotNet;

public static class OrderedDictionaryExtensions
{
    public static void Swap<TKey, TValue>(this global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int index1, int index2)
        where TKey : notnull
    {
        Check.NotNull(dictionary);
        SwapCore(dictionary, index1, index2);
    }

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
