namespace FclEx.Comparers;

public class FileExtentionEqualityComparer : IEqualityComparer<string>, IHasInstance<FileExtentionEqualityComparer>
{
    public bool Equals(string? x, string? y)
    {
        if (EqualityComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var subx = x.SkipUntil(".", untilLast: true);
        var suby = y.SkipUntil(".", untilLast: true);

        return string.Equals(subx, suby, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        return obj
            .SkipUntil(".", untilLast: true)
            .GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public static FileExtentionEqualityComparer Instance { get; } = new();
}