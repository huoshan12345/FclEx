using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FclEx.Wmi.SourceGenerator.Extensions;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid([NotNullWhen(true)] this string? x) => !x.IsNullOrEmpty();

    public static string TrimStart(this string str, string prefix, StringComparison comp = StringComparison.Ordinal)
    {
        return str.IsValid() && prefix.IsValid() && str.StartsWith(prefix, comp) ? str.Substring(prefix.Length) : str;
    }
}