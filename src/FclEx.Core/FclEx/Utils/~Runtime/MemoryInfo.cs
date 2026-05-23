#if NET6_0_OR_GREATER

namespace FclEx.Utils;

public readonly struct MemoryInfo
{
    private readonly GCMemoryInfo _data;

    public MemoryInfo(GCMemoryInfo data)
    {
        _data = data;
    }

    /// <summary>
    /// High memory load threshold when this GC occured
    /// </summary>
    public long HighMemoryLoadThresholdBytes => _data.HighMemoryLoadThresholdBytes;

    /// <summary>
    /// Memory load when this GC occurred
    /// </summary>
    public long MemoryLoadBytes => _data.MemoryLoadBytes;

    /// <summary>
    /// Total available memory for the GC to use when this GC occurred.
    ///
    /// If the environment variable COMPlus_GCHeapHardLimit is set,
    /// or "Server.GC.HeapHardLimit" is in runtimeconfig.json, this will come from that.
    /// If the program is run in a container, this will be an implementation-defined fraction of the container's size.
    /// Else, this is the physical memory on the machine that was available for the GC to use when this GC occurred.
    /// </summary>
    public long TotalAvailableMemoryBytes => _data.TotalAvailableMemoryBytes;

    /// <summary>
    /// The total heap size when this GC occurred
    /// </summary>
    public long HeapSizeBytes => _data.HeapSizeBytes;

    /// <summary>
    /// The total fragmentation when this GC occurred
    ///
    /// Let's take the example below:
    ///  | OBJ_A |     OBJ_B     | OBJ_C |   OBJ_D   | OBJ_E |
    ///
    /// Let's say OBJ_B, OBJ_C and OBJ_E are garbage and get collected, but the heap does not get compacted, the resulting heap will look like the following:
    ///  | OBJ_A |           F           |   OBJ_D   |
    ///
    /// The memory between OBJ_A and OBJ_D marked `F` is considered part of the FragmentedBytes, and will be used to allocate new objects. The memory after OBJ_D will not be
    /// considered part of the FragmentedBytes, and will also be used to allocate new objects
    /// </summary>
    public long FragmentedBytes => _data.FragmentedBytes;

    /// <summary>
    /// The index of this GC. GC indices start with 1 and get increased at the beginning of a GC.
    /// Since the info is updated at the end of a GC, this means you can get the info for a BGC
    /// with a smaller index than a foreground GC finished earlier.
    /// </summary>
    public long Index => _data.Index;

    /// <summary>
    /// The generation this GC collected. Collecting a generation means all its younger generation(s)
    /// are also collected.
    /// </summary>
    public int Generation => _data.Generation;

    /// <summary>
    /// Is this a compacting GC or not.
    /// </summary>
    public bool Compacted => _data.Compacted;

    /// <summary>
    /// Is this a concurrent GC (BGC) or not.
    /// </summary>
    public bool Concurrent => _data.Concurrent;

    /// <summary>
    /// Total committed bytes of the managed heap.
    /// </summary>
    public long TotalCommittedBytes => _data.TotalCommittedBytes;

    /// <summary>
    /// Promoted bytes for this GC.
    /// </summary>
    public long PromotedBytes => _data.PromotedBytes;

    /// <summary>
    /// Number of pinned objects this GC observed.
    /// </summary>
    public long PinnedObjectsCount => _data.PinnedObjectsCount;

    /// <summary>
    /// Number of objects ready for finalization this GC observed.
    /// </summary>
    public long FinalizationPendingCount => _data.FinalizationPendingCount;

    /// <summary>
    /// This is the % pause time in GC so far. If it's 1.2%, this number is 1.2.
    /// </summary>
    public double PauseTimePercentage => _data.PauseTimePercentage;

    public static MemoryInfo Get() => new(GC.GetGCMemoryInfo());
}

#endif