namespace FclEx.Build;

/// <summary>
/// Simple msbuild project file loader.
/// </summary>
[Obsolete("Use  Microsoft.Build instead.")]
public static class ProjectFileLoader
{
    public const string PropsFileName = "Directory.Build.props";
    public const string TargetsFileName = "Directory.Build.targets";

    private static readonly ConcurrentDictionary<string, string> _cache = new();

    private static async Task<string> ReadFile(FileInfo file)
    {
        var key = file.FullName;
        if (_cache.TryGetValue(key, out var text))
            return text;

        text = await File.ReadAllTextAsync(key);
        _cache[key] = text;
        return text;
    }

    private static async Task<XElement> LoadProjectFile(FileInfo file)
    {
        if (file.Exists == false)
            throw new FileNotFoundException(file.FullName);

        // TODO: convert all relative paths to absolute ones.
        var text = await ReadFile(file);
        var doc = XDocument.Parse(text);
        var root = doc.Root ?? throw new InvalidOperationException("Xml file does not have a root node: " + file.Name);

        var list = new List<(XElement, XElement)>();
        foreach (var element in root.Elements())
        {
            if (element.Name.LocalName == "Import")
            {
                var path = element.Attribute("Project")?.Value
                           ?? throw new InvalidOperationException("Missing attribute 'Project' for 'Import'.");

                if (path.StartsWith('.'))
                {
                    path = Path.Combine(file.FullName, "..", path);
                }

                var import = new FileInfo(path);

                if (import.Exists == false)
                    throw new InvalidOperationException("The importing file does not exist: " + import.FullName);

                var imported = await LoadProjectFile(import);
                list.Add((element, imported));
            }
        }

        foreach (var (element, imported) in list)
        {
            var children = imported.Elements().Cast<object>().ToArray();
            element.ReplaceWith(children);
        }

        return root;
    }

    private static string[]? GetTargetFrameworks(XElement xml)
    {
        const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

        string[]? frameworks = null;
        foreach (var element in xml.Elements("PropertyGroup"))
        {
            foreach (var sub in element.Elements("TargetFrameworks"))
            {
                frameworks = sub.Value.Split(";", options);
            }
        }
        return frameworks;
    }

    private static async Task LoadProjectFiles(DirectoryInfo dir)
    {
        if (dir.Exists == false)
            throw new DirectoryNotFoundException(dir.FullName);

        var shared = await new[] { TargetsFileName, PropsFileName }
            .Select(m => Path.Combine(dir.FullName, m))
            .Select(m => new FileInfo(m))
            .Where(m => m.Exists)
            .Select(m => LoadProjectFile(m))
            .WhenAll();

        foreach (var sub in dir.EnumerateDirectories().OrderBy(m => m.Name))
        {
            var projectFile = sub.EnumerateFiles("*.csproj").SingleOrDefault();
            if (projectFile is null)
                continue;

            var xml = await LoadProjectFile(projectFile);

            foreach (var item in shared)
            {
                var children = item.Elements().Cast<object>().ToArray();
                xml.AddFirst(children);
            }

            var frameworks = GetTargetFrameworks(xml);

            if (frameworks.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot find TargetFrameworks in file: " + projectFile.Name);
        }
    }
}