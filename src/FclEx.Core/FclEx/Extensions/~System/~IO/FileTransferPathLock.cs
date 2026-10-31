namespace FclEx.Extensions;

// File-copy publication and file moves must share the same lock for each destination path.
// Retaining every visited path would grow the registry indefinitely, so entries are reference-counted
// and removed once neither a holder nor a waiter needs them. This coordinates this process only.
internal static class FileTransferPathLock
{
    private static readonly Dictionary<string, Entry> _entries = new(
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal);

    // Protect only registry membership and reference counts; each path has its own AsyncLock.
    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
    _lock = new();

    internal static IDisposable Acquire(string path)
    {
        path = Path.GetFullPath(path);
        var entry = AddReference(path);
        AsyncLock.Lease? acquiredLock = null;
        try
        {
            // Waiting must happen outside the registry lock so other paths can proceed independently.
            acquiredLock = entry.Lock.Acquire();
            return new Lease(path, entry, acquiredLock);
        }
        catch
        {
            acquiredLock?.Dispose();
            ReleaseReference(path, entry);
            throw;
        }
    }

    internal static async Task<IDisposable> AcquireAsync(string path, CancellationToken cancellationToken = default)
    {
        path = Path.GetFullPath(path);
        var entry = AddReference(path);
        AsyncLock.Lease? acquiredLock = null;
        try
        {
            // Async callers wait without occupying a thread and can cancel before acquiring the path.
            acquiredLock = await entry.Lock.AcquireAsync(cancellationToken).ConfigureAwait(false);
            return new Lease(path, entry, acquiredLock);
        }
        catch
        {
            // A canceled waiter still owns a registry reference, even though it never acquired the lock.
            acquiredLock?.Dispose();
            ReleaseReference(path, entry);
            throw;
        }
    }

    private static Entry AddReference(string path)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(path, out var entry))
                _entries.Add(path, entry = new Entry());

            // Register before waiting. If only holders were counted, an exiting holder could remove
            // the entry while a waiter still uses it, allowing a new caller to create a second lock
            // for the same path and enter concurrently with that waiter.
            entry.Users++;
            return entry;
        }
    }

    private static void ReleaseReference(string path, Entry entry)
    {
        lock (_lock)
        {
            // Lookup, registration, and removal share the same lock, so no caller can acquire
            // an uncounted reference between the zero check and removal.
            if (--entry.Users == 0)
                _entries.Remove(path);
        }
    }

    private sealed class Entry
    {
        internal AsyncLock Lock { get; } = new();

        // Holders plus sync/async waiters; access is protected by the registry lock, not the path lock.
        internal int Users;
    }

    // AsyncLock leases have no thread affinity; disposal may run on a different continuation thread.
    private sealed class Lease(string path, Entry entry, AsyncLock.Lease acquiredLock) : IDisposable
    {
        private Entry? _entry = entry;

        public void Dispose()
        {
            // Release the lock and its registry reference only once, including concurrent disposal.
            if (Interlocked.Exchange(ref _entry, null) is not { } current)
                return;

            // Keep the reference until after releasing the path lock so the registry cannot replace
            // this entry while its protected operation is still running.
            acquiredLock.Dispose();
            ReleaseReference(path, current);
        }
    }
}
