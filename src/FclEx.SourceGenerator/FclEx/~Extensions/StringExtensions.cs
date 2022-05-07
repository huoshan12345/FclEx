using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FclEx
{
    public static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string JoinWith(this IEnumerable<string> strs, string separator = "")
            => string.Join(separator, strs);
    }
}
