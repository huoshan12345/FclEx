namespace System.Collections.Generic;

public class SecureStringEqualityComparer : IEqualityComparer<SecureString>
{
    public static readonly SecureStringEqualityComparer Instance = new();

    public bool Equals(SecureString? x, SecureString? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        using var disposable1 = MarshalHelper.SecureStringToBSTR(x);
        using var disposable2 = MarshalHelper.SecureStringToBSTR(y);

        var str1 = Marshal.PtrToStringBSTR(disposable1.Value);
        var str2 = Marshal.PtrToStringBSTR(disposable2.Value);

        return string.Equals(str1, str2, StringComparison.Ordinal);
    }

    public int GetHashCode(SecureString obj)
    {
        using var disposable = MarshalHelper.SecureStringToBSTR(obj);
        var str = Marshal.PtrToStringBSTR(disposable.Value);
        return str?.GetHashCode() ?? 0;
    }
}
