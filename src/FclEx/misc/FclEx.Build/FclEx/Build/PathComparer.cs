namespace FclEx.Build;

public class PathComparer : IComparer<string>
{
    public static PathComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (ComparerHelper.TryCompare(x, y, out var result))
            return result.Value;

        var sections1 = GetNameSections(x);
        var sections2 = GetNameSections(y);

        using var e1 = sections1.AsEnumerable().GetEnumerator();
        using var e2 = sections2.AsEnumerable().GetEnumerator();

        while (true)
        {
            var l = e1.MoveNext();
            var r = e2.MoveNext();
            if (l && r)
            {
                var compare = CompareSections(e1.Current, e2.Current);
                if (compare != 0)
                    return compare;
            }
            else if (l)
            {
                return 1; // longer is larger
            }
            else if (r)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    private static int CompareSections(string[] x, string[] y)
    {
        using var e1 = x.AsEnumerable().GetEnumerator();
        using var e2 = y.AsEnumerable().GetEnumerator();

        while (true)
        {
            var l = e1.MoveNext();
            var r = e2.MoveNext();

            if (l && r)
            {
                var result = string.Compare(e1.Current, e2.Current, StringComparison.OrdinalIgnoreCase);
                if (result != 0)
                    return result;
            }
            else if (l)
            {
                return 1; // longer is larger
            }
            else if (r)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    private static readonly ConcurrentDictionary<string, string[][]> _cache = new();
    private static string[][] GetNameSections(string path)
    {
        return _cache.GetOrAdd(path, m =>
            m.Split(Path.DirectorySeparatorChar)
                .Select(x => x.Split('.'))
                .ToArray());
    }
}