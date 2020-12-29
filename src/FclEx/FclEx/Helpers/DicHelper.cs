using System.Collections.Generic;

namespace FclEx.Helpers
{
    public static class DicHelper<TKey, TValue> where TKey : notnull
    {
        public static IReadOnlyDictionary<TKey, TValue> Empty { get; } = new Dictionary<TKey, TValue>();
    }
}
