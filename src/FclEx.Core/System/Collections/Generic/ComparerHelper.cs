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

    /// <summary>
    /// Determines equality for identical references and null values, and optionally for values whose runtime types differ.
    /// </summary>
    /// <typeparam name="T">The declared type of the values.</typeparam>
    /// <param name="x">The first value.</param>
    /// <param name="y">The second value.</param>
    /// <param name="result">The determined result, or <see langword="null"/> when the caller must compare the values.</param>
    /// <param name="requireSameRuntimeType">
    /// Whether non-null values of different runtime types are immediately considered unequal. Defaults to
    /// <see langword="true"/> to preserve the traditional strict comparison behavior.
    /// </param>
    /// <returns><see langword="true"/> when <paramref name="result"/> was determined; otherwise, <see langword="false"/>.</returns>
    public static bool TryEquals<T>(
        [NotNullWhen(false), NoEnumeration] T? x,
        [NotNullWhen(false), NoEnumeration] T? y,
        [NotNullWhen(true)] out bool? result,
        bool requireSameRuntimeType = true)
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
        else if (requireSameRuntimeType && x.GetType() != y.GetType())
        {
            result = false;
        }

        return result.HasValue;
    }
}
