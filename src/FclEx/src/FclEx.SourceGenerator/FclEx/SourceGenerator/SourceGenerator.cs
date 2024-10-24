namespace FclEx.SourceGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator, IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();

        context.RegisterPostInitializationOutput(i =>
        {
            var codes = new[]
            {
                MethodHelperSource.Generate(),
                TypeExtensionsSource.Generate(),
                ValueTupleExtensionsSource.Generate(),
                TupleExtensionsSource.Generate(),
                EventHandlersSource.Generate(),
                AsyncEventHandlerExtensionsSource.Generate(),
                UnicodeScalarHelperSource.Generate(),
            };

            foreach (var (file, code) in codes)
            {
                i.AddSource(file, code);
            }
        });
    }
}