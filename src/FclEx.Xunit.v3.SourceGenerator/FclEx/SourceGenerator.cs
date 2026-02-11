#pragma warning disable RS1035

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using FclEx.CodeAnalysis;
using FclEx.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FclEx;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public static readonly Assembly Assembly = typeof(SourceGenerator).Assembly;
    public static readonly string AssemblyName = Assembly.GetName().Name;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        var attribute = context.SyntaxProvider.ForAttributeWithMetadataName(
                "FclEx.Xunit.XunitSerializableAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
            );

        context.RegisterImplementationSourceOutput(attribute, Generate_XunitSerializable);

        context.RegisterImplementationSourceOutput(context.CompilationProvider, Generate_FclExXunitHelper);
    }

    private static void Generate_XunitSerializable(SourceProductionContext context, INamedTypeSymbol serializableTypeSymbol)
    {
        var ns = serializableTypeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : serializableTypeSymbol.ContainingNamespace.ToDisplayString();

        var typeName = serializableTypeSymbol.Name;
        var fullTypeName = serializableTypeSymbol.ToDisplayString();

        var source = $$"""
using System;
using System.Reflection;
using Xunit.Sdk;

{{(ns is not null ? $"namespace {ns};" : "")}}

public partial {{(serializableTypeSymbol.IsRecord ? "record" : "class")}} {{typeName}} : IXunitSerializable
{
    private static readonly FieldInfo[] _fields = FclExXunitHelper.GetAllFields(typeof({{typeName}}));

    public void Serialize(IXunitSerializationInfo info)
    {
        foreach (var field in _fields)
        {
            var value = field.GetValue(this);
            info.AddValue(field.Name, value, field.FieldType);
        }
    }

    public void Deserialize(IXunitSerializationInfo info)
    {
        foreach (var field in _fields)
        {
            var value = info.GetValue(field.Name, field.FieldType);
            field.SetValue(this, value);
        }
    }
}
""";

        context.AddSource($"{typeName}.XunitSerializable.g.cs", source);
    }

    private static void Generate_FclExXunitHelper(SourceProductionContext context, Compilation compilation)
    {
        const string fileBaseName = "FclExXunitHelper";
        var text = Assembly.GetManifestResourceText("FclEx", "Xunit", $"{fileBaseName}.cs");
        context.AddSource($"{fileBaseName}.g.cs", text);
    }
}

file static class AssemblyExtensions
{
    public static string GetManifestResourceText(this Assembly assembly, params string[] paths)
    {
        var name = assembly.GetName().Name;
        var path = paths.Prepend(name).JoinWith(".");
        using var stream = assembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException(path);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();

    }
}