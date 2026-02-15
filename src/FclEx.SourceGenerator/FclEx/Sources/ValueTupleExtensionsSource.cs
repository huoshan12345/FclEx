namespace FclEx.Sources;

internal static class ValueTupleExtensionsSource
{
    private const int Max = 8;
    private static readonly string[] _usings =
    [
        "System",
        "System.Reflection",
    ];

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "ValueTupleExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteLine("#nullable enable")
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public partial class {className}")
            .WriteOpeningBracket();

        for (var i = 2; i <= Max; i++)
        {
            const string methodName = "public static string? FirstNotEmpty";
            builder.WriteLine("[return: NotNullIfNotNull(nameof(defaultValue))]");
            var types = Enumerable.Repeat("string?", i).JoinWith(", ");
            builder.WriteLine($"{methodName}(this ({types}) tuple, string? defaultValue = \"\")");
            builder.WriteOpeningBracket();
            builder.WriteLine($"using var disposable = ArrayPool<string?>.Shared.GetPooled({i});");
            builder.WriteLine("var arr = disposable.Value;");
            for (var j = 0; j < i; j++)
            {
                builder.WriteLine($"arr[{j}] = tuple.Item{j + 1};");
            }
            builder.WriteLine($"return arr.FirstNotEmpty({i}, defaultValue);");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        for (var i = 2; i <= Max; i++)
        {
            const string methodName = "public static IEnumerable<T> Yield<T>";
            var types = Enumerable.Repeat("T", i).JoinWith(", ");
            builder.WriteLine($"{methodName}(this ({types}) tuple)");
            builder.WriteOpeningBracket();
            for (var j = 0; j < i; j++)
            {
                builder.WriteLine($"yield return tuple.Item{j + 1};");
            }
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
