namespace FclEx.Comparers;

public static class ComparerHelper
{
    public static bool TryCompare<T>(
        [NotNullWhen(false), NoEnumeration] T? x,
        [NotNullWhen(false), NoEnumeration] T? y,
        bool isNullSmaller,
        [NotNullWhen(true)] out int? result,
        bool useReferenceEquals = true)
    {
        result = null;

        if (useReferenceEquals && ReferenceEquals(x, y))
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

    public static bool TryEquals<T>(
        [NotNullWhen(false), NoEnumeration] T? x,
        [NotNullWhen(false), NoEnumeration] T? y,
        [NotNullWhen(true)] out bool? result,
        bool useReferenceEquals = true)
    {
        result = null;

        if (useReferenceEquals && ReferenceEquals(x, y))
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