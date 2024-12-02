using FclEx.Utils;

namespace FclEx.Build;

public class SRGenerator
{
    [LocalOnlyTheory]
    [InlineData(
        new[]
        {
            @"D:\projects-origin\dotnet\runtime\src\libraries\System.Private.CoreLib\src\Resources\Strings.resx",
            @"D:\projects-origin\dotnet\runtime\src\libraries\System.Collections\src\Resources\Strings.resx",
        },
        @"FclEx\src\FclEx.Core\System\Collections",
        "System.Collections")]
    public async Task GenerateAsync(string[] sources, string target, string @namespace)
    {
        var src = AppContext.BaseDirectory.TakeUntil("src");
        var targetDir = new DirectoryInfo(Path.Combine(src, target));
        Assert.True(targetDir.Exists, targetDir.FullName);

        var dic = new SortedDictionary<string, string>();
        foreach (var source in sources)
        {
            var sourceFile = new FileInfo(source);
            Assert.True(sourceFile.Exists, sourceFile.FullName);
            var text = await File.ReadAllTextAsync(sourceFile.FullName);
            var doc = XDocument.Parse(text);
            var root = doc.Root ?? throw new InvalidOperationException("Xml file does not have a root node: " + sourceFile.Name);

            foreach (var element in root.Elements().Where(m => m.Name == "data"))
            {
                var data = element.Attribute("name")?.Value
                           ?? throw new InvalidOperationException("The element does not have a 'name' attribute.");
                var value = element.Element("value")?.Value
                            ?? throw new InvalidOperationException("The element does not have a 'value' child.");

                dic[data] = value;
            }
        }

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteEnableNullable()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine("public static class SR")
            .WriteOpeningBracket();

        const string template = """
                                public static string Format([StringSyntax("CompositeFormat")] string format, params object?[] args)
                                {
                                    return string.Format(format, args);
                                }
                                """;
        builder.WriteAsLines(template);
        builder.WriteLine();

        foreach (var (key, value) in dic)
        {
            var v = value.Trim('"');
            var quote = v.Contains('"')
                ? "\"\"\""
                : "\"";
            builder.WriteLine($"public const string {key} = {quote}{v}{quote};");
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        var targetPath = Path.Combine(targetDir.FullName, "SR.cs");
        await File.WriteAllTextAsync(targetPath, str);
    }
}