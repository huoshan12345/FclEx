#pragma warning disable IDE0005
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
#pragma warning restore IDE0005

namespace FclEx.Extensions;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string JoinWith(this IEnumerable<string> enumerable, string separator = "")
        => string.Join(separator, enumerable);

    public static string Replace(this Capture capture, string input, Func<string, string?> evaluator)
    {
        var replacement = evaluator(capture.Value) ?? string.Empty;
        if (replacement == capture.Value)
            return input;

        var str = input[..capture.Index] + replacement + input[(capture.Index + capture.Length)..];
        return str;
    }
}