#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace System.Collections.Generic;

/// <summary>
/// Compares objects by reading the bytes stored in their instance memory.
/// </summary>
/// <remarks>
/// This comparer intentionally supports reference types as well as value types, but it compares runtime object memory
/// rather than logical equality. Results can depend on CLR object layout, padding bytes, reference field addresses,
/// runtime version, platform architecture, and implementation details that may change between executions.
/// Use it only when those object-memory semantics are explicitly desired.
/// </remarks>
public unsafe class ObjectMemoryEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly ObjectMemoryEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var p1 = &x;
        var p2 = &y;

        if (p1 == p2)
            return true;

        var span1 = AsSpan(p1);
        var span2 = AsSpan(p2);

        return span1.SequenceEqual(span2);
    }

    public int GetHashCode(T? obj)
    {
        if (obj is null)
            return 0;

        var p = &obj;
        var span = AsSpan(p);
        return span.ComputeHashCode();
    }

    private static Span<byte> AsSpan(T?* pointer)
    {
        var size = SizeCalculator.SizeOf<T>();

        // For reference types, the pointer points to the address of the target object (i.e., a double pointer).
        // Therefore, it needs to be converted into an IntPtr* pointer, and the contents of the pointer (i.e., the address of the target object) must be resolved.
        // This address points to the Method Table of the object.
        // Moving this address forward by one unit (IntPtr.Size) gives the starting address of the memory segment where the instance resides, which is the address of the Object Header.
        // Moving this address backward by one unit (IntPtr.Size) gives the address of the first member variable of the instance.
        var (dataSize, dataAddress) = typeof(T).IsValueType switch
        {
            true => (size, new IntPtr(pointer)),
            false => (size - 2 * IntPtr.Size, *(IntPtr*)pointer + IntPtr.Size),
        };

        var span = new Span<byte>(dataAddress.ToPointer(), dataSize);
        return span;
    }

    internal static byte[] GetBytes(T? obj)
    {
        if (obj is null)
            return [];

        var p = &obj;
        var span = AsSpan(p);
        return span.ToArray();
    }
}
