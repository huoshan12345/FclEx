using System;

namespace FclEx.Sources;

internal static class BytesExtensionsSource
{
    private static readonly string[] _usings =
    [
        "System",
    ];

    private static readonly string[] _types =
    [
        nameof(Boolean),
        nameof(Char),
        nameof(Int16),
        nameof(UInt16),
        nameof(Int32),
        nameof(UInt32),
        nameof(Int64),
        nameof(UInt64),
        nameof(Single),
        nameof(Double),
    ];

    internal static IEnumerable<SourceInfo> Generate()
    {
        yield return GenerateForByteArray();
        yield return GenerateForReadOnlySpan();
    }

    private static SourceInfo GenerateForByteArray()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "BytesExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        foreach (var type in _types)
        {
            const string methodName = "public static byte[] ToBytes";
            builder.WriteLine($"{methodName}(this {type} value)");
            builder.WriteOpeningBracket();
            builder.WriteLine("return BitConverter.GetBytes(value);");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        foreach (var type in _types)
        {
            var methodName = $"public static {type} To{type}";
            builder.WriteLine($"{methodName}(this byte[] bytes, int offset = 0)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return BitConverter.To{type}(bytes, offset);");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.WriteLine($"{methodName}(this byte[] bytes, ref int offset)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"offset += sizeof({type});");
            builder.WriteLine($"return BitConverter.To{type}(bytes, offset);");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }

    private static SourceInfo GenerateForReadOnlySpan()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "BytesExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        foreach (var type in _types)
        {
            var methodName = $"public static {type} To{type}";
            builder.WriteLine($"{methodName}(this ReadOnlySpan<byte> span)");
            builder.WriteOpeningBracket();

            builder.WriteIf("NET6_0_OR_GREATER");
            builder.WriteLine($"return BitConverter.To{type}(span);");
            builder.WriteElse();
            builder.WriteLine($"return Unsafe.ReadUnaligned<{type}>(ref MemoryMarshal.GetReference(span));");
            builder.WriteEndIf();
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.ReadOnlySpan.g.cs", str);
    }
}