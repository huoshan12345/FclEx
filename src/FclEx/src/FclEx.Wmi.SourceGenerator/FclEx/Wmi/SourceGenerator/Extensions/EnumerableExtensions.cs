using System;
using System.Collections.Generic;

namespace FclEx.Wmi.SourceGenerator.Extensions;

public static class EnumerableExtensions
{
    // Extension for MoreLinq.Index()
    public static IEnumerable<(T item, int index, bool isFirst, bool isLast)> IndexExt<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null)
        {
            throw new ArgumentNullException(nameof(enumerable));
        }

        // we separate the null check from the method body with yield, otherwise the null check will not be executed until start enumerating.
        // see details in https://stackoverflow.com/questions/42149895/method-having-yield-return-is-not-throwing-exception
        return WithIndexBody(enumerable);

        static IEnumerable<(T item, int index, bool isFirst, bool isLast)> WithIndexBody(IEnumerable<T> enumerable)
        {
            using var enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                yield break;
            }

            var i = 0;
            var current = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (current, i, i == 0, false);
                current = enumerator.Current;
                ++i;
            }

            yield return (current, i, i == 0, true);
        }
    }
}