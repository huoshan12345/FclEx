using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FclEx.Extensions;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith(this IEnumerable<string> enumerable, string separator = "")
        => string.Join(separator, enumerable);
}