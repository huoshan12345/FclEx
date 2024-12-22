namespace System.Collections.Generic;

public class MarshalToBytesEqualityComparer<T> : IEqualityComparer<T>
{
    public static readonly MarshalToBytesEqualityComparer<T> Instance = new();

    public bool Equals(T? x, T? y)
    {
        typeof(T).EnsureMarshalable();

        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var bytes1 = x.MarshalToBytes();
        var bytes2 = y.MarshalToBytes();
        return bytes1.SequenceEqual(bytes2);
    }

    public int GetHashCode(T? obj)
    {
        typeof(T).EnsureMarshalable();

        if (obj is null)
            return 0;

        var bytes = obj.MarshalToBytes();
        return bytes.ComputeHashCode();
    }
}