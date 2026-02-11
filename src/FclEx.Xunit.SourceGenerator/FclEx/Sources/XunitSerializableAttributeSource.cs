using System;
using System.Collections.Generic;
using System.Linq;
using FclEx.CodeAnalysis;
using FclEx.Extensions;
using FclEx.Utils;
using Microsoft.CodeAnalysis;

namespace FclEx.Sources;

internal static class XunitSerializableAttributeSource
{
    private static readonly string[] _usings =
    [
        "System",
        "System.Reflection",
#if FCLEX_XUNIT_V3
        "Xunit.Sdk",
#else
        "Xunit.Abstractions",
#endif
    ];

    private static string GetTypeDef(INamedTypeSymbol typeSymbol)
    {
        var typeDefs = new List<string>();
        if (typeSymbol.IsRecord)
            typeDefs.Add("record");
        typeDefs.Add(typeSymbol.IsValueType ? "struct" : "class");
        return typeDefs.JoinWith(" ");
    }

    internal static SourceInfo Generate(INamedTypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        var typeName = typeSymbol.Name;
        var typeDef = GetTypeDef(typeSymbol);

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteEnableNullable()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        if (ns is not null)
        {
            // Namespace declaration
            builder.WriteNamespace(ns, true)
                .WriteLine();
        }

        var nestedCount = 0;
        var bastType = typeSymbol.ContainingType;
        while (bastType != null)
        {
            nestedCount++;

            builder.WriteLine($"partial {GetTypeDef(bastType)} {bastType.Name}");
            builder.WriteOpeningBracket();

            bastType = bastType.ContainingType;
        }

        builder.WriteLine($"partial {typeDef} {typeName} : IXunitSerializable");
        builder.WriteOpeningBracket();

        builder.WriteLine($"private static readonly IReadOnlyList<FieldInfo> _fields = FclEx.Extensions.TypeExtensions.GetAllInstanceFields(typeof({typeName}));");
        builder.WriteLine();

        const string getValue =
#if FCLEX_XUNIT_V3
            "info.GetValue(field.Name)";
#else
            "info.GetValue(field.Name, field.FieldType)";
#endif

        const string methods = $$"""
public void Serialize(IXunitSerializationInfo info)
{
    foreach (var field in _fields)
    {
        var value = field.GetValue(this);
        info.AddValue(field.Name, value, value?.GetType());
    }
}

public void Deserialize(IXunitSerializationInfo info)
{
    foreach (var field in _fields)
    {
        var value = {{getValue}};
        field.SetValue(this, value);
    }
}
""";

        builder.WriteLines(methods);

        if (typeSymbol.InstanceConstructors.Any(c => c.Parameters.Length == 0) == false)
        {
            // generate parameterless constructor

            var ctor = typeSymbol.InstanceConstructors
                .FirstOrDefault(c => c.IsImplicitlyDeclared == false && c.Parameters.Length > 0);

            if (ctor is null)
                throw new InvalidOperationException($"Type {typeSymbol.ToDisplayString()} does not have any declared constructors.");

            var args = ctor.Parameters.Select(p => "default!");
            var argsStr = args.JoinWith(", ");

            builder.WriteLine();
            builder.WriteLine($$"""public {{typeSymbol.Name}}() : this({{argsStr}}) { }""");
        }

        // End class declaration
        builder.WriteClosingBracket();

        for (var i = 0; i < nestedCount; i++)
        {
            builder.WriteClosingBracket();
        }

        var str = builder.ToString();
        return ($"{typeName}.g.cs", str);
    }
}
