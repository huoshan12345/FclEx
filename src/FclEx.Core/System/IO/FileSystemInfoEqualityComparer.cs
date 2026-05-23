namespace System.IO;

public sealed class FileSystemInfoEqualityComparer : IEqualityComparer<FileSystemInfo?>
{
    public static readonly FileSystemInfoEqualityComparer CaseSensitive = new(PathComparison.CaseSensitive);
    public static readonly FileSystemInfoEqualityComparer CaseInsensitive = new(PathComparison.CaseInsensitive);
    public static readonly FileSystemInfoEqualityComparer Auto = new();

    private readonly StringComparer _comparer;

    public FileSystemInfoEqualityComparer(PathComparison comparison = PathComparison.Auto)
    {
        _comparer = comparison switch
        {
            PathComparison.CaseSensitive => StringComparer.Ordinal,
            PathComparison.CaseInsensitive => StringComparer.OrdinalIgnoreCase,
            PathComparison.Auto => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison))
        };
    }

    public bool Equals(FileSystemInfo? x, FileSystemInfo? y)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (ComparerHelper.TryEquals(x, y, out var result))
            return result.Value;

        return _comparer.Equals(x.FullName, y.FullName);
    }

    public int GetHashCode(FileSystemInfo? obj)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (obj is null)
            return 0;

        return _comparer.GetHashCode(obj.FullName);
    }
}
