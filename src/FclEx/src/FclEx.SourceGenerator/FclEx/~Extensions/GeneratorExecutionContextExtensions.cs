using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace FclEx
{
    public static class GeneratorExecutionContextExtensions
    {
        public static readonly string AssemblyName = Assembly.GetCallingAssembly().GetName().Name!;

        public static void ReportDiagnostic(this GeneratorExecutionContext context, Exception ex, DiagnosticSeverity severity = DiagnosticSeverity.Error, string? messagePrefix = null)
        {
            messagePrefix ??= "An exception was thrown by the " + AssemblyName;
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    id: "SI0000",
                    title: messagePrefix,
                    messageFormat: messagePrefix + ": '{0}'",
                    category: AssemblyName,
                    defaultSeverity: severity,
                    isEnabledByDefault: true),
                Location.None,
                ex.ToString()));
        }
    }
}
