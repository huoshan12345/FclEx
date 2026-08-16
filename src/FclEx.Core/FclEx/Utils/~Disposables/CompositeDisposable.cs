namespace FclEx.Utils;

/// <summary>
/// Represents a collection of disposable objects that can be disposed as a single unit.
/// </summary>
/// <typeparam name="T">The type of disposable objects in the collection.</typeparam>
/// <remarks>
/// CompositeDisposable provides a way to group multiple <see cref="IDisposable"/> objects
/// and dispose them all with a single call. This is useful for managing
/// related resources that need to be released together.
/// Instance members are not thread-safe.
/// </remarks>
public class CompositeDisposable<T> : IDisposable where T : IDisposable
{
    private List<T>? _disposables;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDisposable{T}"/> class
    /// with the specified collection of disposable objects.
    /// </summary>
    /// <param name="enumerable">The collection of disposable objects to include in this composite.</param>
    /// <remarks>
    /// The enumerable is materialized immediately to ensure the objects
    /// are captured at construction time and not affected by subsequent changes to the source.
    /// </remarks>
    public CompositeDisposable(IEnumerable<T>? enumerable)
    {
        _disposables = enumerable?.ToList() ?? [];
    }

    /// <summary>
    /// Adds a disposable object to this composite.
    /// </summary>
    /// <param name="disposable">The disposable object whose lifetime is owned by this composite.</param>
    /// <returns>This composite instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">This composite has already been disposed.</exception>
    public CompositeDisposable<T> Add(T disposable)
    {
        Check.NotNull(disposable);

        if (_disposables is null)
            throw new ObjectDisposedException(GetType().FullName);

        _disposables.Add(disposable);
        return this;
    }

    /// <summary>
    /// Disposes every owned object exactly once.
    /// </summary>
    /// <exception cref="AggregateException">One or more owned objects failed to dispose.</exception>
    /// <remarks>
    /// Disposal is idempotent. All owned objects are given an opportunity to dispose even when an earlier object throws.
    /// </remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        var disposables = _disposables;
        if (disposables is null)
            return;

        _disposables = null;

        List<Exception>? exceptions = null;
        foreach (var disposable in disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception exception)
            {
                (exceptions ??= []).Add(exception);
            }
        }

        if (exceptions is not null)
            throw new AggregateException("One or more disposables failed to dispose.", exceptions);
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
