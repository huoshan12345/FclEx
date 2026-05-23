using Microsoft.CodeAnalysis.CSharp;

namespace FclEx.Tests;

public static class StringExtensions
{
    /// <summary>
    /// Returns a C# string literal with the given value.
    /// </summary>
    /// <returns>A string literal with the given value.</returns>
    /// <remarks>
    /// Escapes non-printable characters.
    /// </remarks>
    public static string ToLiteral(this string value)
    {
        return SymbolDisplay.FormatLiteral(value, false);
    }

    public static string ToLiteral(this char value)
    {
        return value.ToString().ToLiteral();
    }
}