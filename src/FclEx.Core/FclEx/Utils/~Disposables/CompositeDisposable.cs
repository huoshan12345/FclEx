namespace FclEx.Utils;

/// <summary>
/// Represents a collection of disposable objects that can be disposed as a single unit.
/// </summary>
/// <typeparam name="T">The type of disposable objects in the collection.</typeparam>
/// <remarks>
/// CompositeDisposable provides a way to group multiple <see cref="IDisposable"/> objects
/// and dispose them all with a single call. This is useful for managing
/// related resources that need to be released together.
/// </remarks>
public class CompositeDisposable<T> : IDisposable where T : IDisposable
{
    private readonly List<T> _disposables;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable{T}"/> class
    /// with the specified collection of disposable objects.
    /// </summary>
    /// <param name="enumerable">The collection of disposable objects to include in this composite.</param>
    /// <remarks>
    /// The enumerable is materialized immediately to an array to ensure the objects
    /// are captured at construction time and not affected by subsequent changes to the source.
    /// </remarks>
    public CompositeDisposable(IEnumerable<T>? enumerable)
    {
        _disposables = enumerable?.AsList() ?? [];
    }

    public CompositeDisposable<T> Add(T disposable)
    {
        _disposables.Add(disposable);
        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var e in _disposables)
        {
            e.Dispose();
        }
    }
}

public class CompositeDisposable(IEnumerable<IDisposable>? enumerable)
    : CompositeDisposable<IDisposable>(enumerable);

public static class CompositeDisposableExtensions
{
    /// <summary>
    /// Merges multiple <see cref="IDisposable"/> objects into a single CompositeDisposable.
    /// </summary>
    /// <typeparam name="T">The type of disposable objects.</typeparam>
    /// <param name="enumerable">The collection of disposable objects to merge.</param>
    /// <returns>A CompositeDisposable that, when disposed, will dispose all the contained objects.</returns>
    public static CompositeDisposable<T> Merge<T>(this IEnumerable<T> enumerable) where T : IDisposable
    {
        return new CompositeDisposable<T>(enumerable);
    }
}