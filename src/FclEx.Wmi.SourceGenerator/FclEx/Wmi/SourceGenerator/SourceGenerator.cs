using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using FclEx.Wmi.SourceGenerator.Extensions;
using FclEx.Wmi.SourceGenerator.Models;
using FclEx.Wmi.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;

namespace FclEx.Wmi.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                ExecuteInternal(context);
            }
            catch (Exception ex)
            {
                //This is temporary till https://github.com/dotnet/roslyn/issues/46084 is fixed
                context.ReportDiagnostic(ex);
            }
        }

        private static readonly string[] Namespaces =
        {
            @"Root\CIMV2",
        };

        private static IEnumerable<string> LoadNamespaces(GeneratorExecutionContext context)
        {
            var queue = new Queue<string>();
            queue.Enqueue("root");

            while (queue.Any())
            {
                var cur = queue.Dequeue();
                ManagementClass nsClass;
                try
                {
                    nsClass = new ManagementClass(new ManagementScope(cur), new ManagementPath("__namespace"), null);
                }
                catch (ManagementException ex)
                {
                    context.ReportDiagnostic(ex, DiagnosticSeverity.Warning, "Failed to get classes from wmi namespace " + cur);
                    continue;
                }

                foreach (var ns in nsClass.GetInstances())
                {
                    var nsName = $"{cur}\\{ns["Name"]}";
                    yield return nsName;
                    queue.Enqueue(nsName);
                }
            }
        }

        private static IEnumerable<ClassItem> LoadClasses(string namespaceName)
        {
            var ns = new ManagementScope(namespaceName);
            var searcher = new ManagementObjectSearcher(ns, new WqlObjectQuery("SELECT * FROM meta_class"));
            foreach (var wmiClass in searcher.Get())
            {
                var className = wmiClass["__CLASS"].ToString()!;
                var mClass = new ManagementClass(ns, new ManagementPath(className), null)
                {
                    Options = { UseAmendedQualifiers = true }
                };
                var (desc, qualifiers) = GetQualifierData(mClass.Qualifiers);
                var classItem = new ClassItem(className)
                {
                    Description = desc,
                    Qualifiers = qualifiers
                };
                foreach (var property in mClass.Properties)
                {
                    var item = new PropertyItem(property.Name, property.Type, property.IsArray)
                    {
                        Description = GetQualifierData(property.Qualifiers).Description,
                    };
                    classItem.Properties.Add(item);
                }
                yield return classItem;
            }
        }

        private static (string Description, List<string> Qualifiers) GetQualifierData(QualifierDataCollection collection)
        {
            var descriptionList = new List<string>
            {
                "Description:"
            };
            var qualifierList = new List<string>();

            foreach (var entry in collection)
            {
                qualifierList.Add(entry.Name);

                if ("description".Equals(entry.Name, StringComparison.OrdinalIgnoreCase) && entry.Value is string value)
                {
                    descriptionList.Add(value);
                }
            }
            var description = string.Join(Environment.NewLine, descriptionList) + Environment.NewLine
                + "Qualifiers:" + Environment.NewLine + string.Join(", ", qualifierList);
            return (description, qualifierList);
        }

        //By not inlining we make sure we can catch assembly loading errors when jitting this method
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ExecuteInternal(GeneratorExecutionContext context)
        {
            foreach (var ns in Namespaces)
            {
                foreach (var @class in LoadClasses(ns).Where(m => !m.Qualifiers.Contains("abstract")))
                {
                    var (name, code) = ClassItemSource.Generate(ns, @class);
                    context.AddSource(name, code);
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
            Debug.WriteLine("Initalize code generator");
#endif
        }
    }
}
