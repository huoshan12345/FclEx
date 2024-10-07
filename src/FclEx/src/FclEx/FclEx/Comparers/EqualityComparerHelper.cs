namespace FclEx.Comparers;

public static class EqualityComparerHelper
{
    public static bool TryEquals<T>([NotNullWhen(false), NoEnumeration] T? x, [NotNullWhen(false), NoEnumeration] T? y, [NotNullWhen(true)] out bool? result)
    {
        result = null;

        if (ReferenceEquals(x, y))
        {
            result = true;
        }
        else if (x is null || y is null)
        {
            result = false;
        }

        return result.HasValue;
    }
}