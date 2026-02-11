#pragma warning disable RS1035

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FclEx.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FclEx.Xunit;

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

    private static void Generate_XunitSerializable(SourceProductionContext context, INamedTypeSymbol typeSymbol)
    {
        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace.ToDisplayString();

        var typeName = typeSymbol.Name;
        var fullTypeName = typeSymbol.ToDisplayString();
        var typeDeclare = $"partial {(typeSymbol.IsRecord ? "record" : "class")} {typeName}";

        var source = $$"""
using System;
using System.Reflection;
using Xunit.Sdk;
using FclEx.Xunit;

{{(ns is not null ? $"namespace {ns};" : "")}}

{{typeDeclare}} : IXunitSerializable
{
    private static readonly FieldInfo[] _fields = FclExXunitHelper.GetAllFields(typeof({{typeName}})).ToArray();

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
            var value = info.GetValue(field.Name);
            field.SetValue(this, value);
        }
    }
}
""";

        if (typeSymbol.InstanceConstructors.Any(c => c.Parameters.Length == 0) == false)
        {
            // generate parameterless constructor

            var ctor = typeSymbol.InstanceConstructors
                .FirstOrDefault(c => c.IsImplicitlyDeclared == false && c.Parameters.Length > 0);

            if (ctor is null)
                throw new InvalidOperationException($"Type {fullTypeName} does not have any declared constructors.");

            var args = ctor.Parameters.Select(p => "default!");
            var argsStr = args.JoinWith(", ");

            source += Environment.NewLine * 2;

            source += $$"""
{{typeDeclare}}
{
    public {{typeSymbol.Name}}() : this({{argsStr}}) { }
}
""";
        }

        context.AddSource($"{typeName}.XunitSerializable.g.cs", source);
    }

    private static void Generate_FclExXunitHelper(SourceProductionContext context, Compilation compilation)
    {
        const string fileBaseName = "FclExXunitHelper";
        var text = Assembly.GetManifestResourceText("FclEx", "Xunit", $"{fileBaseName}.cs");
        context.AddSource($"{fileBaseName}.g.cs", text);
    }
}

file static class Extensions
{
    public static string GetManifestResourceText(this Assembly assembly, params string[] paths)
    {
        var filePath = paths.JoinWith(".");
        var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(m => m.EndsWith(filePath, StringComparison.Ordinal)) ?? throw new FileNotFoundException(filePath);
        using var stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    extension(string)
    {
        public static string operator *(string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }
    }
}