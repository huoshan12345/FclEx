namespace System.Collections.Generic;

public class SecureStringEqualityComparer : IEqualityComparer<SecureString>
{
    public static readonly SecureStringEqualityComparer Instance = new();

    /// <summary>
    /// Compares two secure strings by converting them to managed strings and using ordinal string comparison.
    /// </summary>
    /// <remarks>
    /// The comparison temporarily materializes each value as a managed <see cref="string" />. That copy is controlled by
    /// the GC and cannot be cleared deterministically, so use this comparer only when that exposure is acceptable.
    /// </remarks>
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

    /// <summary>
    /// Computes an ordinal hash code for the secure string contents.
    /// </summary>
    /// <remarks>
    /// This method temporarily materializes the value as a managed <see cref="string" />. That copy is controlled by the
    /// GC and cannot be cleared deterministically, so use this comparer only when that exposure is acceptable.
    /// </remarks>
    public int GetHashCode(SecureString obj)
    {
        using var disposable = MarshalHelper.SecureStringToBSTR(obj);
        var str = Marshal.PtrToStringBSTR(disposable.Value);
        return str?.GetHashCode(StringComparison.Ordinal) ?? 0;
    }
}
