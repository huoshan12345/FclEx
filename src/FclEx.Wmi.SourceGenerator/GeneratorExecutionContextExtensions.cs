using System;
using Microsoft.CodeAnalysis;

namespace FclEx.Wmi.SourceGenerator
{
    public static class GeneratorExecutionContextExtensions
    {
        public static readonly string AssemblyName = typeof(GeneratorExecutionContextExtensions).Assembly.GetName().Name!;

        public static void ReportDiagnostic(this GeneratorExecutionContext context, Exception ex, DiagnosticSeverity severity = DiagnosticSeverity.Error, string? messagePrefix = null)
        {
            messagePrefix ??= "An exception was thrown by the StrongInject generator";
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    id: "SI0000",
                    title: "An exception was thrown by the StrongInject generator",
                    messageFormat: messagePrefix + ": '{0}'",
                    category: AssemblyName,
                    defaultSeverity: severity,
                    isEnabledByDefault: true),
                Location.None,
                ex.ToString()));
        }
    }
}
