namespace System.Collections.Generic;

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