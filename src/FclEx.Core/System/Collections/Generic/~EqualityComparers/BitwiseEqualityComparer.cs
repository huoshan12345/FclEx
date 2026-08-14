namespace System.Collections.Generic;

/// <summary>
/// Compares unmanaged values by their complete in-memory binary representation.
/// </summary>
/// <remarks>
/// The comparison includes padding bytes. Consequently, values that are logically equal can compare as unequal when
/// their binary representations differ, such as positive and negative floating-point zero. The representation can
/// also vary by architecture and endianness.
/// </remarks>
public sealed unsafe class BitwiseEqualityComparer<T> : IEqualityComparer<T> where T : unmanaged
{
    public static readonly BitwiseEqualityComparer<T> Instance = new();

    public bool Equals(T x, T y)
    {
        return AsBytes(ref x).SequenceEqual(AsBytes(ref y));
    }

    public int GetHashCode(T obj)
    {
        return AsBytes(ref obj).ComputeHashCode();
    }

    private static ReadOnlySpan<byte> AsBytes(ref T value)
    {
        return new(Unsafe.AsPointer(ref value), sizeof(T));
    }
}
