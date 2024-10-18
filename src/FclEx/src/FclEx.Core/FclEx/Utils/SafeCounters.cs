namespace FclEx.Utils;

// ReSharper disable once UnusedMember.Global
public class SafeCounters<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, SafeCounter> _counters = new();

    public SafeCounter Get(TKey key) => _counters.GetOrAdd(key, m => new SafeCounter());

    public SafeCounter this[TKey key] => Get(key);
}

public class SafeCounters : SafeCounters<string> { }