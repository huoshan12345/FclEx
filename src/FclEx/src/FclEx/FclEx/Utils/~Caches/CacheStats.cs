using System.Threading;

namespace FclEx.Utils;

public class CacheStats
{
    private volatile int _missedCount;
    private volatile int _hitCount;

    public int MissedCount => _missedCount;

    public int HitCount => _hitCount;

    public void OnHit()
    {
        Interlocked.Increment(ref _hitCount);
    }

    public void OnMiss()
    {
        Interlocked.Increment(ref _missedCount);
    }
}