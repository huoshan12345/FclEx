namespace FclEx.SourceGenerator.Sources;

internal class TupleExtensionsSource
{
    private const int Max = 7;
    private static readonly string[] _usings =
    {
        "System",
        "System.Reflection",
    };

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "TupleExtensions";

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
        builder.WriteLine($"partial class {className}")
            .WriteOpeningBracket();

        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");
            var methodName = $"public static ({types}) ToValueTuple<{types}>";
            builder.WriteLine($"{methodName}(this Tuple<{types}> tuple)");
            builder.WriteOpeningBracket();
            var result = Enumerable.Range(1, i).Select(m => $"tuple.Item{m}").JoinWith(", ");
            builder.WriteLine($"return ({result});");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");
            var methodName = $"public static IEnumerable<({types})> ToValueTuple<{types}>";
            builder.WriteLine($"{methodName}(this IEnumerable<Tuple<{types}>> enumerable)");
            builder.WriteOpeningBracket();
            builder.WriteLine("return enumerable.Select(m => m.ToValueTuple());");
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
