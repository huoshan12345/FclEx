using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FclEx.CodeAnalysis;
using FclEx.Wmi.SourceGenerator.Extensions;
using FclEx.Wmi.SourceGenerator.Models;
using FclEx.Wmi.SourceGenerator.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
#pragma warning disable RS1041

namespace FclEx.Wmi.SourceGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
    private static readonly bool IsGithubAction = Environment.GetEnvironmentVariable("GITHUB_ACTION") is { Length: > 0 };
    private static readonly bool IsWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static void GenerateToFiles(string? folder)
    {
        var di = new DirectoryInfo(folder ?? Path.Combine(".", "Generated"));
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

    public void Initialize(GeneratorInitializationContext context)
    {
        //if (Debugger.IsAttached == false)
        //    Debugger.Launch();
    }

    private static readonly string[] Namespaces =
    [
        @"Root\CIMV2",
    ];

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
                nsClass = new ManagementClass(new ManagementScope(cur), new ManagementPath("__namespace"), new());
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
            var qualifiers = Read(wmiClass.Qualifiers);
            var classItem = new ClassItem(className, qualifiers);
            foreach (var property in wmiClass.Properties)
            {
                var q = Read(property.Qualifiers);
                var item = new PropertyItem(property.Name, property.Type, property.IsArray, q);
                classItem.Properties.Add(item);
            }
            yield return classItem;
        }
    }

    private static readonly Regex _dot = new(@"\. ", RegexOptions.Compiled);
    private static readonly char[] LineSeparators = ['\r', '\n'];
    private static Qualifiers Read(QualifierDataCollection collection)
    {
        var qualifiers = new Qualifiers();
        foreach (var entry in collection)
        {
            if (entry.Value is not string value)
                continue;

            var name = entry.Name.ToLower();
            if (name == "description")
            {
                var lines = _dot.Replace(value, ".\r\n")
                    .Split(LineSeparators)
                    .Select(m => m.Trim())
                    .Where(m => m.IsValid());

                foreach (var line in lines)
                {
                    qualifiers.Descriptions.Add(line);
                }
            }
            else
            {
                qualifiers.Others.Add(name, value.Trim());
            }
        }
        return qualifiers;
    }

    private static IEnumerable<SourceInfo> Generate()
    {
        foreach (var ns in Namespaces)
        {
            var query = LoadClasses(ns)
                .Where(m => !m.Qualifiers.Others.ContainsKey("abstract"))
                .GroupBy(m => GetFirstChar(m.Name));

            foreach (var group in query)
            {
                var info = ClassItemSource.Generate(ns, group, group.Key.ToString());
                yield return info;
            }
        }
        yield break;

        static char GetFirstChar(string name)
        {
            const string prefix = "Win32_";
            var index = name.IndexOf("Win32_", StringComparison.Ordinal);
            var ch = index >= 0
                ? name[index + prefix.Length]
                : name[0];
            return char.ToLower(ch);
        }
    }

    private static DirectoryInfo GetResourcesDir(GeneratorExecutionContext context)
    {
        var options = context.AnalyzerConfigOptions;
        const string key = "build_property.projectdir";
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        var path = options?.GetGlobalOption(key) ?? AppContext.BaseDirectory;
        var index = path.IndexOf("src", StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException($"Cannot locate src directory from current path: {path}");
        }

        var assembly = typeof(SourceGenerator).Assembly.GetName().Name!;
        var projectDir = Path.Combine(path[..index], "src", "FclEx", "src", assembly);
        if (Directory.Exists(projectDir) == false)
        {
            throw new InvalidOperationException($"Source generator project directory does not exist: {projectDir}");
        }

        var resourcesDir = new DirectoryInfo(Path.Combine(projectDir, "Resources"));
        if (resourcesDir.Exists == false)
        {
            resourcesDir.Create();
            resourcesDir.Refresh();
        }
        return resourcesDir;
    }

    private static (List<SourceInfo> Sources, DateTime? Oldest, DirectoryInfo ResourcesDir) ReadFiles(GeneratorExecutionContext context)
    {
        var resourcesDir = GetResourcesDir(context);

        var list = new List<SourceInfo>();
        DateTime? min = default;
        foreach (var file in resourcesDir.EnumerateFiles("*.cs"))
        {
            var text = File.ReadAllText(file.FullName);
            list.Add((file.Name, text));
            if (min is null || min.Value > file.LastWriteTimeUtc)
                min = file.LastWriteTimeUtc;
        }
        return (list, min, resourcesDir);
    }

    //By not inlining we make sure we can catch assembly loading errors when jitting this method
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ExecuteInternal(OutputOptions options, GeneratorExecutionContext context)
    {
        var (fileSources, oldest, resourcesDir) = ReadFiles(context);

        if (IsGithubAction || IsWin == false)
        {
            if (fileSources.Count == 0)
            {
                throw new InvalidOperationException($"There is no cached source file at {resourcesDir.FullName}");
            }
        }

        var needToGenerate = fileSources.Count == 0 || IsWin && IsGithubAction == false && oldest < DateTime.UtcNow.AddMonths(-1);

        var sources = needToGenerate
            ? Generate()
            : fileSources;

        foreach (var (_, name, code) in sources)
        {
            switch (options.OutputType)
            {
                case OutputType.File:
                    var fi = new FileInfo(Path.Combine(options.Folder ?? ".", name));
                    fi.Directory!.Create();
                    File.WriteAllText(fi.FullName, code);
                    break;
                case OutputType.Context:
                default:
                    context.AddSource(name, code);
                    break;
            }

            var path = Path.Combine(resourcesDir.FullName, name);
            File.WriteAllText(path, code);
        }
    }
}