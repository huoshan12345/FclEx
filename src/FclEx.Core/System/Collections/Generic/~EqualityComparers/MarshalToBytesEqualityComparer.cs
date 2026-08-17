namespace System.Collections.Generic;

/// <summary>
/// Compares values by their unmanaged representations produced by the runtime interop marshaler.
/// </summary>
/// <remarks>
/// <typeparamref name="T"/> is validated by <see cref="TypeExtensions.EnsureMarshalable(Type)"/> before each
/// comparison or hash calculation. The native buffer is cleared before every marshal operation, so bytes in padding
/// are consistently zero. This does not make the result a canonical value representation.
///
/// The comparer is unreliable for structural equality when marshaling produces a pointer instead of inline data.
/// This includes pointer-based <see cref="MarshalAsAttribute"/> forms such as <see cref="UnmanagedType.LPStr"/>,
/// <see cref="UnmanagedType.LPWStr"/>, <see cref="UnmanagedType.BStr"/>, <see cref="UnmanagedType.LPArray"/>,
/// interface pointers, and custom marshalers. The produced bytes contain an address, which may differ across marshal
/// operations even for the same value. The bytes also depend on the current platform ABI and must not be used as a
/// portable or persistent representation.
/// </remarks>
public class MarshalToBytesEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly MarshalToBytesEqualityComparer<T> Instance = new();
    
    public bool Equals(T? x, T? y)
    {
        typeof(T).EnsureMarshalable(); // do not put it in the static constructor

        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var bytes1 = Marshal.ToBytes(x, clearNativeBuffer: true);
        var bytes2 = Marshal.ToBytes(y, clearNativeBuffer: true);
        return bytes1.SequenceEqual(bytes2);
    }

    public int GetHashCode(T? obj)
    {
        typeof(T).EnsureMarshalable();

        if (obj is null)
            return 0;

        var bytes = Marshal.ToBytes(obj, clearNativeBuffer: true);
        return bytes.ComputeHashCode();
    }
}
