namespace FclEx.Comparers;

public class FileExtensionEqualityComparer : IEqualityComparer<string>, IHasInstance<FileExtensionEqualityComparer>
{
    public bool Equals(string? x, string? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        var x1 = x.SkipUntil(".", untilLast: true);
        var y1 = y.SkipUntil(".", untilLast: true);

        return string.Equals(x1, y1, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        return obj
            .SkipUntil(".", untilLast: true)
            .GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public static FileExtensionEqualityComparer Instance { get; } = new();
}