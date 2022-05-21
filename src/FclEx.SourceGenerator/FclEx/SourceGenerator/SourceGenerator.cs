using System;
using System.Diagnostics;
using FclEx.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace FclEx.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var codes = new[]
                {
                    MethodHelperSource.Generate(),
                    TypeExtensionsSource.Generate(),
                };

                foreach (var (file, code) in codes)
                {
                    context.AddSource(file, code);
                }
            }
            catch (Exception ex)
            {
                //This is temporary till https://github.com/dotnet/roslyn/issues/46084 is fixed
                context.ReportDiagnostic(ex);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//            Debug.WriteLine("Initalize code generator");
//#endif
        }
    }
}
