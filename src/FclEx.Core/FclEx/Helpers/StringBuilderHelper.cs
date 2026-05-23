using static System.Text.StringBuilderCache;

namespace FclEx.Helpers;

public static class StringBuilderHelper
{
    public static string Build(Action<StringBuilder> action)
    {
        using var disposable = GetCached();
        var builder = disposable.Value;
        action(builder);
        return builder.ToString();
    }

    /// <summary>
    /// Acquires a thread-local cached <see cref="StringBuilder"/> instance with the specified capacity,
    /// wrapped in a <see cref="DisposableValue{T}"/> for automatic release back to the cache upon disposal.
    /// </summary>
    /// <param name="capacity">
    /// The minimum capacity of the <see cref="StringBuilder"/> to acquire. Defaults to 16.
    /// </param>
    /// <returns>
    /// A <see cref="DisposableValue{T}"/> containing a <see cref="StringBuilder"/>. Disposing the value releases the instance back to the cache.
    /// </returns>
    public static DisposableValue<StringBuilder> GetCached(int capacity = 16) // == StringBuilder.DefaultCapacity
    {
        return Disposable.FromValue(Acquire(capacity), Release);
    }
}