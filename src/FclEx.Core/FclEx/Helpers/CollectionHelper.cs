using System.Collections.ObjectModel;

namespace FclEx.Helpers;

public static class CollectionHelper
{
    public static IReadOnlyCollection<T> EmptyReadOnlyCol<T>() => [];
    public static IReadOnlyList<T> EmptyReadOnlyList<T>() => [];
    public static ReadOnlyDictionary<TKey, TValue> EmptyReadOnlyDictionary<TKey, TValue>() where TKey : notnull
        => Cache<TKey, TValue>.EmptyDic;

    internal class Cache<TKey, TValue> where TKey : notnull
    {
        public static readonly ReadOnlyDictionary<TKey, TValue> EmptyDic
            = new(new Dictionary<TKey, TValue>());
    }
}