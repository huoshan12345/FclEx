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

    // Protect only registry membership and reference counts; path operations use their own monitors.
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
        Entry entry;
        lock (_lock)
        {
            if (!_entries.TryGetValue(path, out entry!))
                _entries.Add(path, entry = new Entry());

            // Register before waiting. If only holders were counted, an exiting holder could remove
            // the entry while a waiter still uses it, allowing a new caller to create a second lock
            // for the same path and enter concurrently with that waiter.
            entry.Users++;
        }

        var entered = false;
        try
        {
            // Never wait while holding the registry lock: a busy path must not block other paths
            // from acquiring locks or releasing their references.
            Monitor.Enter(entry, ref entered);
            return new Lease(path, entry);
        }
        catch
        {
            // Balance the registration even if waiting is interrupted or lease creation fails.
            if (entered)
                Monitor.Exit(entry);
            ReleaseReference(path, entry);
            throw;
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
        // Holders plus waiters; access is protected by the registry lock, not this entry's monitor.
        internal int Users;
    }

    // Acquisition and disposal must run on the same thread; never hold a lease across await.
    private sealed class Lease(string path, Entry entry) : IDisposable
    {
        private Entry? _entry = entry;

        public void Dispose()
        {
            if (_entry is not { } current)
                return;

            // Keep the reference until after leaving the monitor so the registry cannot replace
            // this entry while its protected operation is still running.
            Monitor.Exit(current);
            _entry = null;
            ReleaseReference(path, current);
        }
    }
}
