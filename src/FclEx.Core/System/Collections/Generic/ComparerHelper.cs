namespace System.Collections.Generic;

public static class ComparerHelper
{
    public static bool TryCompare<T>([NotNullWhen(false), NoEnumeration] T? x, [NotNullWhen(false), NoEnumeration] T? y, [NotNullWhen(true)] out int? result)
    {
        result = null;

        if (ReferenceEquals(x, y))
        {
            result = 0;
        }
        else if (x is null)
        {
            result = -1;
        }
        else if (y is null)
        {
            result = 1;
        }

        return result.HasValue;
    }

    public static bool TryEquals<T>([NotNullWhen(false), NoEnumeration] T? x, [NotNullWhen(false), NoEnumeration] T? y, [NotNullWhen(true)] out bool? result)
    {
        result = null;

        if (ReferenceEquals(x, y))
        {
            result = true;
        }
        // ReSharper disable once DuplicatedChainedIfBodies
        else if (x is null || y is null)
        {
            result = false;
        }
        else if (x.GetType() != y.GetType())
        {
            result = false;
        }

        return result.HasValue;
    }
}