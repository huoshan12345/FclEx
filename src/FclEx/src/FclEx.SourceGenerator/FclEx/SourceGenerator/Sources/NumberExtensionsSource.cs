using System;
using System.Collections.Generic;
using System.Text;

namespace FclEx.SourceGenerator.Sources;

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
        builder.WriteLine($"partial class {className}")
            .WriteOpeningBracket();

        foreach (var type in _types)
        {
            var template = $$"""
                            public static {{type}} RoundUp(this {{type}} number, {{type}} @base)
                            {
                                Check.NotLessThan<{{type}}>(number, 0);
                                Check.GreaterThan<{{type}}>(@base, 0);
                            
                                var remaining = ({{type}})(number % @base);
                                var value = remaining == 0
                                    ? number
                                    : number + (@base - remaining);
                                return ({{type}})value;
                            }
                            """;
            builder.WriteAsLines(template);
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