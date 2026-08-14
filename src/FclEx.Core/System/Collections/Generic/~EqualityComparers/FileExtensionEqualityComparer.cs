namespace System.Collections.Generic;

public sealed class FileExtensionEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        return StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(x), Path.GetExtension(y));
    }

    public int GetHashCode(string obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Path.GetExtension(obj));
    }

    public static FileExtensionEqualityComparer Instance { get; } = new();
}
