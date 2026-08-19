using FclEx.Sources.Abp;
using FclEx.Sources.DependencyInjection;
using FclEx.Sources.Http;
using FclEx.Sources.Xunit;

namespace FclEx;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        var provider = context.CompilationProvider.Combine(context.AnalyzerConfigOptionsProvider);
        context.RegisterImplementationSourceOutput(provider, (ctx, value) =>
        {
            var (compilation, options) = value;
            var assembly = compilation.AssemblyName;
            var codes = assembly switch
            {
                "FclEx.Abp" => GenerateAbpSources(),
                "FclEx.Core" =>
                [
                    ..BytesExtensionsSource.Generate(),
                    MethodInfoExtensionsSource.Generate(),
                    TypeExtensionsSource.Generate(),
                    ValueTupleExtensionsSource.Generate(),
                    TupleExtensionsSource.Generate(),
                    EventHandlersSource.Generate(),
                    AsyncEventHandlerExtensionsSource.Generate(),
                    UnicodeScalarHelperSource.Generate(ctx, value.Right),
                    NumberExtensionsSource.Generate(),
                    StringBuilderExtensionsSource.Generate(),
                    HashExtensionsSource.Generate(),
                    OperationResultExtensionsSource.Generate(),
                ],
                "FclEx.Http" =>
                [
                    HttpClientBuilderExtensionsSource.Generate(),
                ],
                "FclEx.Xunit" =>
                [
                    ExtensionsSource.Generate(),
                ],
                "FclEx.Xunit.v3" =>
                [
                    ExtensionsSource.Generate(),
                    TheoryDataRowExtensionsSource.Generate(),
                ],
                "FclEx.DependencyInjection" =>
                [
                    ServiceCollectionExtensionsSource.Generate(),
                ],
                _ => [],
            };

            if (codes.Any(m => m.Success == false))
                return;

            if (codes.Length == 0)
            {
                Report("No code generated for assembly: {0}", assembly);
                return;
            }

            foreach (var (_, file, code) in codes)
            {
                ctx.AddSource(file, code);
            }

            void Report(string messageFormat, params object?[]? args)
            {
                var descriptor = new DiagnosticDescriptor(
                    id: "FclEx",
                    title: nameof(context.RegisterImplementationSourceOutput),
                    messageFormat: messageFormat,
                    category: nameof(SourceGenerator),
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true);
                ctx.ReportDiagnostic(Diagnostic.Create(descriptor, null, messageArgs: args));
            }

            SourceInfo[] GenerateAbpSources()
            {
                const string abpCoreAssemblyName = "Volo.Abp.Core";
                var abpCoreAssembly = compilation.SourceModule.ReferencedAssemblySymbols.FirstOrDefault(a => a.Name == abpCoreAssemblyName);
                // ReSharper disable once InvertIf
                if (abpCoreAssembly is null)
                {
                    Report("Cannot find referenced assembly: {0}", abpCoreAssemblyName);
                    return [];
                }

                return
                [
                    AbpCoreUsingsSource.Generate(abpCoreAssembly),
                ];
            }
        });
    }
}