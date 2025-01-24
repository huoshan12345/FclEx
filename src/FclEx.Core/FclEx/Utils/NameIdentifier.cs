#if NET6_0_OR_GREATER
#pragma warning disable CA2252
namespace FclEx.Utils;

public interface INameIdentifier<out T> where T : INameIdentifier<T>
{
    string Name { get; }
    static abstract T Create(string name);
}

public abstract record NameIdentifier<T>(string Name) where T : NameIdentifier<T>, INameIdentifier<T>
{
    private static readonly ConcurrentDictionary<string, T> _cache = new();

    public static T GetOrCreate(string name, bool useCache = true)
    {
        return useCache 
            ? _cache.GetOrAdd(name, T.Create) 
            : T.Create(name);
    }

    public void ClearCache() => _cache.Clear();
    public override string ToString() => Name;
    public override int GetHashCode() => Name.GetHashCode();
}
#endif
