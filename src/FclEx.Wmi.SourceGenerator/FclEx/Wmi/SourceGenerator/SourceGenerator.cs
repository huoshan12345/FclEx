using System;
using System.Collections.Generic;
using System.IO;
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
        public static void Generate(string folder)
        {
            var di = new DirectoryInfo(folder);
            if (di.Exists)
            {
                di.Delete(true);
                di.Create();
            }
            ExecuteInternal(new OutputOptions(OutputType.File, di.FullName), default);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                ExecuteInternal(new OutputOptions(OutputType.Context, null), context);
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
            var ns = new ManagementScope(namespaceName, new ConnectionOptions { Locale = "MS_409" });
            var searcher = new ManagementObjectSearcher(ns, new WqlObjectQuery("SELECT * FROM Meta_Class WHERE __Class LIKE \"Win32_%\" AND NOT __Class LIKE \"Win32_Perf%\""), new() { UseAmendedQualifiers = true });
            foreach (var wmiClass in searcher.Get().Cast<ManagementClass>())
            {
                var className = wmiClass.Path.ClassName;
                var (desc, qualifiers) = GetQualifierData(wmiClass.Qualifiers);
                var classItem = new ClassItem(className)
                {
                    Description = desc,
                    Qualifiers = qualifiers
                };
                foreach (var property in wmiClass.Properties)
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
            var descriptionList = new List<string>();
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
                + "Qualifiers: " + string.Join(", ", qualifierList);
            return (description, qualifierList);
        }

        //By not inlining we make sure we can catch assembly loading errors when jitting this method
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ExecuteInternal(OutputOptions options, GeneratorExecutionContext context)
        {
            foreach (var ns in Namespaces)
            {
                foreach (var @class in LoadClasses(ns).Where(m => !m.Qualifiers.Contains("abstract")))
                {
                    var (name, code) = ClassItemSource.Generate(ns, @class);

                    switch (options.OutputType)
                    {
                        case OutputType.File:
                            var fi = new FileInfo(Path.Combine(options.Folder ?? ".", ns, name));
                            fi.Directory!.Create();
                            File.WriteAllText(fi.FullName, code);
                            break;
                        case OutputType.Context:
                        default:
                            context.AddSource(name, code);
                            break;
                    }
                }
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
