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
    public const string AttributeName = "FclEx.Xunit.XunitSerializableAttribute";

    private static readonly string[] _usings =
    [
        "System",
        "System.Reflection",
        "System.Runtime.CompilerServices",
        "Xunit.Sdk",
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

        var typeDef = GetTypeDef(typeSymbol);
        var typeName = typeSymbol.Name;
        var usings = _usings;

        if (typeSymbol.IsGenericType)
        {
            var extraUsings = new HashSet<string>(_usings);
            var args = new List<string>();

            foreach (var typeParameter in typeSymbol.TypeParameters)
            {
                args.Add(typeParameter.Name);

                if (typeParameter.ContainingNamespace.Name is { Length: > 0 } namespaceName)
                {
                    extraUsings.Add(namespaceName);
                }
            }

            typeName += $"<{args.JoinWith(", ")}>";

            usings = extraUsings.ToArray();
        }

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteEnableNullable()
            .WriteWarningDisable("CS8500")
            .WriteLine()
            .WriteUsings(usings)
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

        var declaration = typeSymbol.IsValueType
            ? "public"
            : IfBaseTypeImplementsIXunitSerializable(typeSymbol)
                ? "public override"
                : "public virtual";

        var methodSerialize = $$"""
{{declaration}} void Serialize(IXunitSerializationInfo info)
{
    foreach (var field in _fields)
    {
        var value = field.GetValue(this);
        global::Xunit.XunitSerializationInfoExtensions.AddValue(info, field, value);
    }
}
""";

        var methodDeserializeForClass = $$"""
{{declaration}} void Deserialize(IXunitSerializationInfo info)
{
    foreach (var field in _fields)
    {
        var value = global::Xunit.XunitSerializationInfoExtensions.GetValue(info, field);
        field.SetValue(this, value);
    }
}
""";

        var methodDeserializeForStruct = $$"""
{{declaration}} unsafe void Deserialize(IXunitSerializationInfo info)
{
    fixed (void* p = &this)
    {
        ref var r = ref Unsafe.AsRef<{{typeName}}>(p);
        var t = __makeref(r);
        
        foreach (var field in _fields)
        {
            var value = global::Xunit.XunitSerializationInfoExtensions.GetValue(info, field);
            field.SetValueDirect(t, value!);
        }
    }
}
""";

        builder.WriteLines(methodSerialize);
        builder.WriteLine();
        builder.WriteLines(typeSymbol.IsValueType
            ? methodDeserializeForStruct
            : methodDeserializeForClass);

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
        var fileName = typeSymbol.IsGenericType
            ? $"{typeSymbol.Name}.{typeSymbol.TypeParameters.Length}"
            : typeSymbol.Name;
        return ($"{fileName}.g.cs", str);
    }

    private static bool IfBaseTypeImplementsIXunitSerializable(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (baseType.AllInterfaces.Any(i => i.ToDisplayString() == "Xunit.Sdk.IXunitSerializable"))
                return true;

            if (baseType.GetAttributes().Any(m => m.AttributeClass?.ToDisplayString() == AttributeName))
                return true;

            baseType = baseType.BaseType;
        }
        return false;
    }
}
