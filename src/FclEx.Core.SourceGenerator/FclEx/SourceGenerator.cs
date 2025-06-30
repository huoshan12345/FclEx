namespace FclEx;

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
                MethodSource.Generate(),
                TypeExtensionsSource.Generate(),
                ValueTupleExtensionsSource.Generate(),
                TupleExtensionsSource.Generate(),
                EventHandlersSource.Generate(),
                AsyncEventHandlerExtensionsSource.Generate(),
                UnicodeScalarHelperSource.Generate(ctx, provider),
                ..BytesExtensionsSource.Generate(),
                NumberExtensionsSource.Generate(),
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