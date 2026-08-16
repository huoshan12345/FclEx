#if NET6_0_OR_GREATER
#pragma warning disable CA2252
namespace FclEx.Utils;

/// <summary>
/// Defines a contract for name identifiers, providing a standardized way to represent and manage named entities.
/// </summary>
/// <typeparam name="T">The specific type of name identifier, which must implement this interface.</typeparam>
public interface INameIdentifier<out T> where T : INameIdentifier<T>
{
    /// <summary>
    /// Gets the name of the identifier.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Creates a new instance of the name identifier.
    /// </summary>
    /// <param name="name">The name of the identifier.</param>
    /// <returns>A new instance of the name identifier.</returns>
    static abstract T Create(string name);
}

/// <summary>
/// Provides a base implementation for name identifiers, including caching and comparison logic.
/// </summary>
/// <typeparam name="T">The specific type of name identifier, which must inherit from this class and implement <see cref="INameIdentifier{T}"/>.</typeparam>
public abstract record NameIdentifier<T>(string Name) : IComparable<T> where T : NameIdentifier<T>, INameIdentifier<T>
{
    /// <summary>
    /// A cache of name identifiers, keyed by name.
    /// </summary>
    private static readonly ConcurrentDictionary<string, T> _cache = new();

    /// <summary>
    /// Gets an existing name identifier or creates a new one. Uses a cache for efficiency.
    /// </summary>
    /// <param name="name">The name of the identifier.</param>
    /// <param name="useCache">A flag indicating whether to use the cache. Defaults to true.</param>
    /// <returns>An existing or new instance of the name identifier.</returns>
    /// <exception cref="ArgumentException">The factory creates an identifier whose name differs from <paramref name="name"/>.</exception>
    public static T GetOrCreate(string name, bool useCache = true)
    {
        return useCache
            ? _cache.GetOrAdd(name, CreateChecked)
            : CreateChecked(name);

        static T CreateChecked(string name)
        {
            var identifier = Check.NotNull(T.Create(name));
            if (string.Equals(identifier.Name, name, StringComparison.Ordinal) == false)
            {
                throw new ArgumentException(
                    "The name identifier factory must preserve the supplied name.",
                    nameof(name));
            }

            return identifier;
        }
    }

    /// <summary>
    /// Clears the name identifier cache.
    /// </summary>
    public static void ClearCache() => _cache.Clear();
    public sealed override string ToString() => Name;
    public override int GetHashCode() => Name.GetHashCode();

    public int CompareTo(T? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
}
#endif
