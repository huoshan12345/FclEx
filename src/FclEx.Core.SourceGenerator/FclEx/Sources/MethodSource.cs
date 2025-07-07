namespace FclEx.Sources;

internal static class MethodSource
{
    private const int Max = 8;
    private static readonly string[] _usings =
    {
        "System",
        "System.Reflection",
    };

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Helpers";
        const string className = "Method";
        const string methodName = "public static MethodInfo Of";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"partial class {className}")
            .WriteOpeningBracket();

        for (var i = 1; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => "T" + m).JoinWith(", ");
            builder.WriteLine($"{methodName}<{types}>(Action<{types}> action) => action.Method;");
            builder.WriteLine($"{methodName}<{types}, TResult>(Func<{types}, TResult> func) => func.Method;");
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