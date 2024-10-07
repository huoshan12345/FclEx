namespace FclEx.Comparers;

public static class ComparerHelper
{
    public static bool TryCompare<T>([NotNullWhen(false), NoEnumeration] T? x, [NotNullWhen(false), NoEnumeration] T? y, bool isNullSmaller, [NotNullWhen(true)] out int? result)
    {
        result = null;

        if (ReferenceEquals(x, y))
        {
            result = 0;
        }
        else if (x is null)
        {
            result = isNullSmaller ? -1 : 1;
        }
        else if (y is null)
        {
            result = isNullSmaller ? 1 : -1;
        }

        return result.HasValue;
    }
}