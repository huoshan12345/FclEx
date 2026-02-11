#pragma warning disable RS1035

using System;
using System.IO;
using System.Reflection;
using FclEx.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FclEx;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public static readonly string AssemblyName = typeof(SourceGenerator).Assembly.GetName().Name;

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

        context.RegisterImplementationSourceOutput(context.AnalyzerConfigOptionsProvider, Generate_FclExXunitHelper);
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
using Xunit.Abstractions;

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

    private static void Generate_FclExXunitHelper(SourceProductionContext context, AnalyzerConfigOptionsProvider options)
    {
        const string key = "build_property.projectdir";
        var path = options.GetGlobalOption(key);
        if (path is null)
        {
            Report("Cannot find global option by key '{0}'", key);
            return;
        }

        var index = path.IndexOf("src", StringComparison.Ordinal);
        if (index < 0)
        {
            Report("Cannot locate src directory from current path: {0}", path);
            return;
        }

        var projectDir = Path.Combine(path[..index], "src", AssemblyName);
        if (Directory.Exists(projectDir) == false)
        {
            Report("Source generator project directory does not exist: {0}", projectDir);
            return;
        }

        const string fileBaseName = "FclExXunitHelper";
        var file = new FileInfo(Path.Combine(projectDir, "Xunit", $"{fileBaseName}.cs"));
        if (file.Exists == false)
        {
            Report("File does not exist: {0}", file.Name);
            return;
        }

        var text = File.ReadAllText(file.FullName);

        context.AddSource($"{fileBaseName}.g.cs", text);

        void Report(string messageFormat, params object?[]? args)
        {
            var descriptor = new DiagnosticDescriptor(
                id: "FclEx",
                title: AssemblyName,
                messageFormat: messageFormat,
                category: nameof(Generate_FclExXunitHelper),
                defaultSeverity: DiagnosticSeverity.Error,
                isEnabledByDefault: true);
            context.ReportDiagnostic(Diagnostic.Create(descriptor, null, messageArgs: args));
        }
    }
}