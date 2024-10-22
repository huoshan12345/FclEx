using System;
using System.Diagnostics;
using FclEx.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace FclEx.SourceGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator, IIncrementalGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        //#if DEBUG
        //            if (!Debugger.IsAttached)
        //            {
        //                Debugger.Launch();
        //            }
        //            Debug.WriteLine("Initialize code generator");
        //#endif
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(i =>
        {
            var codes = new[]
            {
                MethodHelperSource.Generate(),
                TypeExtensionsSource.Generate(),
                ValueTupleExtensionsSource.Generate(),
                TupleExtensionsSource.Generate(),
            };

            foreach (var (file, code) in codes)
            {
                i.AddSource(file, code);
            }
        });
    }
}