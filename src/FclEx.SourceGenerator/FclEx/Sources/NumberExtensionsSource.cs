using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.Sources;

internal class NumberExtensionsSource
{
    private static readonly string[] _types =
    [
        nameof(SByte),
        nameof(Byte),
        nameof(Int16),
        nameof(UInt16),
        nameof(Int32),
        nameof(UInt32),
        nameof(Int64),
        nameof(UInt64),
    ];

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "NumberExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteIf("!NET7_0_OR_GREATER");

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public partial class {className}")
            .WriteOpeningBracket();

        foreach (var type in _types)
        {
            var template = $$"""
                             /// <summary>
                             /// Rounds up the specified <paramref name="number"/> to the nearest multiple of <paramref name="factor"/>.
                             /// </summary>
                             /// <param name="number">The number to be rounded up.</param>
                             /// <param name="factor">The factor to which <paramref name="number"/> is rounded up.</param>
                             /// <returns>The smallest multiple of <paramref name="factor"/> that is greater than or equal to <paramref name="number"/>.</returns>
                             /// <exception cref="ArgumentException">Thrown if <paramref name="number"/> or <paramref name="factor"/> is less than zero.</exception>
                             public static {{type}} RoundUpTo(this {{type}} number, {{type}} factor)
                             {
                                 Check.NotLessThan<{{type}}>(number, 0);
                                 Check.GreaterThan<{{type}}>(factor, 0);
                             
                                 var remaining = ({{type}})(number % factor);
                                 var value = remaining == 0
                                     ? number
                                     : number + (factor - remaining);
                                 return ({{type}})value;
                             }
                             """;
            builder.WriteLines(template);
            builder.WriteLine();
        }

        foreach (var type in _types)
        {
            var template = $$"""
                             /// <summary>
                             /// Calculates the absolute difference between two numbers.
                             /// </summary>
                             /// <param name="value">The first number.</param>
                             /// <param name="other">The second number to compare with.</param>
                             /// <returns>The absolute difference between <paramref name="value"/> and <paramref name="other"/>.</returns>
                             public static {{type}} AbsDiff(this {{type}} value, {{type}} other)
                             {
                                 return value > other
                                     ? ({{type}})(value - other)
                                     : ({{type}})(other - value);
                             }
                             """;
            builder.WriteLines(template);
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        builder.WriteEndIf();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}