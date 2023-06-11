#nullable enable

using System.Collections.Generic;

namespace FclEx.Comparers;

public class FileExtentionComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is null || y is null)
            return false;

        var subx = x.SkipUntil(".", untilLast: true);
        var suby = y.SkipUntil(".", untilLast: true);

        return string.Equals(subx, suby, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCodeSafely();
    }

    public static readonly FileExtentionComparer Instance = new();
}