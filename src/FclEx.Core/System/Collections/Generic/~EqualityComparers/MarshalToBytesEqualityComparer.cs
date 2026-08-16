namespace System.Collections.Generic;

/// <summary>
/// Compares values by their unmanaged representations produced by the runtime interop marshaler.
/// </summary>
/// <remarks>
/// <typeparamref name="T"/> is validated by <see cref="TypeExtensions.EnsureMarshalable(Type)"/> before each
/// comparison or hash calculation. This comparer is suitable only for types whose marshaled representation consists
/// entirely of inline value data. Padding and pointer-based marshaling can make independently marshaled but otherwise
/// equivalent values compare differently, so types that use pointer-based <see cref="MarshalAsAttribute"/> forms
/// should not be used with this comparer.
/// </remarks>
public class MarshalToBytesEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly MarshalToBytesEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        typeof(T).EnsureMarshalable();

        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var bytes1 = ObjectHelper.MarshalToBytes(x);
        var bytes2 = ObjectHelper.MarshalToBytes(y);
        return bytes1.SequenceEqual(bytes2);
    }

    public int GetHashCode(T? obj)
    {
        typeof(T).EnsureMarshalable();

        if (obj is null)
            return 0;

        var bytes = ObjectHelper.MarshalToBytes(obj);
        return bytes.ComputeHashCode();
    }
}
