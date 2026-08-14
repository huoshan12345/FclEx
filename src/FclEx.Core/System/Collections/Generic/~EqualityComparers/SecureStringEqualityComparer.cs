namespace System.Collections.Generic;

public class SecureStringEqualityComparer : IEqualityComparer<SecureString>
{
    public static readonly SecureStringEqualityComparer Instance = new();

    /// <summary>
    /// Compares the UTF-16 contents of two secure strings without creating managed string copies.
    /// </summary>
    public unsafe bool Equals(SecureString? x, SecureString? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        if (x.Length != y.Length)
            return false;

        using var disposable1 = MarshalHelper.SecureStringToBSTR(x);
        using var disposable2 = MarshalHelper.SecureStringToBSTR(y);

        var left = (char*)disposable1.Value.ToPointer();
        var right = (char*)disposable2.Value.ToPointer();

        for (var i = 0; i < x.Length; i++)
        {
            if (left[i] != right[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Computes an ordinal hash code directly from the secure string's UTF-16 contents.
    /// </summary>
    public unsafe int GetHashCode(SecureString obj)
    {
        Check.NotNull(obj);

        using var disposable = MarshalHelper.SecureStringToBSTR(obj);
        var chars = (char*)disposable.Value.ToPointer();

        unchecked
        {
            var hash = 17;
            for (var i = 0; i < obj.Length; i++)
            {
                hash = hash * 31 + chars[i];
            }
            return hash;
        }
    }
}
