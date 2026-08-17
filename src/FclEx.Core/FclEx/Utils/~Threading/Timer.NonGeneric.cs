namespace FclEx.Utils;

/// <summary>
/// Invokes a parameterless callback on a <see cref="global::System.Threading.Timer"/> schedule.
/// </summary>
/// <remarks>
/// The timer starts during construction. <see cref="Dispose"/> prevents future callbacks but does not wait for a callback
/// that is already queued or running. Use <see cref="DisposeAsync"/> when the caller must wait for those callbacks to finish.
/// </remarks>
public sealed class Timer : IDisposable, IAsyncDisposable
{
    private readonly TimerLifetime _lifetime;

    public Timer(Action callback, TimeSpan dueTime, TimeSpan period)
    {
        Check.NotNull(callback);
        _lifetime = new(new global::System.Threading.Timer(_ => callback(), null, dueTime, period));
    }

    /// <summary>Gets a snapshot of whether disposal has not yet been requested.</summary>
    /// <remarks>A <see langword="true"/> value does not guarantee that a callback is not currently running.</remarks>
    public bool IsActive => _lifetime.IsActive;

    /// <summary>Requests disposal without waiting for queued or running callbacks.</summary>
    public void Dispose() => _lifetime.Dispose();

    /// <summary>Requests disposal and waits until all callbacks queued before disposal have completed.</summary>
    public ValueTask DisposeAsync() => _lifetime.DisposeAsync();
}
