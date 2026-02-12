#pragma warning disable RS1035

using System.Diagnostics;
using System.Reflection;
using FclEx.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FclEx.Xunit;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public static readonly Assembly Assembly = typeof(SourceGenerator).Assembly;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        var serializableType = context.SyntaxProvider.ForAttributeWithMetadataName(
                "FclEx.Xunit.XunitSerializableAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol
            );

        context.RegisterImplementationSourceOutput(serializableType, (ctx, value) =>
        {
            var code = XunitSerializableAttributeSource.Generate(value);
            ctx.AddSource(code.FileName, code.Text);
        });
    }
}