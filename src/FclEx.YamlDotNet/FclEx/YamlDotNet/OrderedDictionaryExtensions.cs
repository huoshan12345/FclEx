namespace FclEx.YamlDotNet;

public static class OrderedDictionaryExtensions
{
    public static void Swap<TKey, TValue>(this global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int index1, int index2)
        where TKey : notnull
    {
        if (index1 == index2)
            return;

        var item1 = dictionary[index1];
        var item2 = dictionary[index2];
        dictionary[index1] = item2;
        dictionary[index2] = item1;
    }

    public static void MoveAt<TKey, TValue>(this global::YamlDotNet.Helpers.IOrderedDictionary<TKey, TValue> dictionary, int oldIndex, int newIndex)
        where TKey : notnull
    {
        Check.NotNull(dictionary);
        Check.Between(oldIndex, 0, dictionary.Count - 1);
        Check.Between(newIndex, 0, dictionary.Count - 1);

        if (oldIndex == newIndex)
            return;

        if (oldIndex < newIndex)
        {
            for (var i = oldIndex; i < newIndex; i++)
            {
                dictionary.Swap(i, i + 1);
            }
        }
        else
        {
            for (var i = oldIndex; i > newIndex; i--)
            {
                dictionary.Swap(i, i - 1);
            }
        }
    }
}
