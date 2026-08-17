namespace System.Collections.Generic;

public class NonGenericDefaultEqualityComparer : IEqualityComparer, IEqualityComparer<object>
{
    private static readonly ConditionalWeakTable<Type, NonGenericDefaultEqualityComparer> _cache = new();

    public static IEqualityComparer Create(Type type)
    {
        return _cache.GetValue(type, m => new NonGenericDefaultEqualityComparer(m));
    }

    private readonly IEqualityComparer _comparer;

    private NonGenericDefaultEqualityComparer(Type type)
    {
        _comparer = typeof(EqualityComparer<>).MakeGenericType(type)
            .GetRequiredProperty(nameof(EqualityComparer<>.Default))
            .GetRequiredValue<IEqualityComparer>(null);
    }

    public new bool Equals(object? x, object? y)
    {
        return ComparerHelper.TryEquals(x, y, out var result)
            ? result.Value
            : _comparer.Equals(x, y);
    }

    public int GetHashCode(object obj)
    {
        return _comparer.GetHashCode(obj);
    }
}
