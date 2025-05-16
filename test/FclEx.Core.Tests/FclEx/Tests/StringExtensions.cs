using Microsoft.CodeAnalysis.CSharp;

namespace FclEx.Tests;

public static class StringExtensions
{
    public static string ToLiteral(this string value)
    {
        return SymbolDisplay.FormatLiteral(value, false);
    }

    public static string ToLiteral(this char value)
    {
        return value.ToString().ToLiteral();
    }
}