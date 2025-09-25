using FclEx.Xunit.Sources;

namespace FclEx.Xunit;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        context.RegisterImplementationSourceOutput(context.AnalyzerConfigOptionsProvider, (ctx, provider) =>
        {
            SourceInfo[] codes =
            [
                ExtensionsSource.Generate(),
            ];

            if (codes.Any(m => m.Success == false))
                return;

            foreach (var (_, file, code) in codes)
            {
                ctx.AddSource(file, code);
            }
        });
    }
}