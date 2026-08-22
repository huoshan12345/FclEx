namespace FclEx.Extensions;

public static class ComparerExtensions
{
    extension(Comparer)
    {
        /// <summary>
        /// Attempts to compare two values using only cheap checks: reference equality
        /// and null checks. Does not perform a full value comparison.
        /// </summary>
        /// <remarks>
        /// Null is treated as less than any non-null value. For value types,
        /// reference equality cannot detect "equal by value" due to boxing, so this
        /// only short-circuits null-related cases in that scenario.
        /// </remarks>
        /// <param name="result">
        /// The comparison result if determined (0 = same reference, -1 = x is null,
        /// 1 = y is null); otherwise null, meaning the caller must compare by value.
        /// </param>
        /// <param name="x">The first value to compare.</param>
        /// <param name="y">The second value to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the result could be determined without a full
        /// value comparison; otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryCompare<T>(
            [NotNullWhen(false), NoEnumeration] T? x,
            [NotNullWhen(false), NoEnumeration] T? y,
            [NotNullWhen(true)] out int? result)
        {
            result = null;

            // check if T is a reference type first to avoid boxing value types for the ReferenceEquals check
            if (default(T) is null && ReferenceEquals(x, y))
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

            // check if T is a reference type first to avoid boxing value types for the ReferenceEquals check
            if (default(T) is null && ReferenceEquals(x, y))
            {
                result = true;
            }
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
}
