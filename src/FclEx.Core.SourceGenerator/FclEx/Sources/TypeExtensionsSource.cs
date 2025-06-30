namespace FclEx.Sources;

internal static class TypeExtensionsSource
{
    private const int Max = 8;
    private static readonly string[] _usings =
    {
        "System",
        "System.Reflection",
    };

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "TypeExtensions";
        const string methodName = "public static Type MakeGenericType";

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
            var types = Enumerable.Range(1, i).Select(m => "T" + m).ToArray();
            var typeParams = types.JoinWith(", ");
            builder.WriteLine($"{methodName}<{typeParams}>(this Type type)");
            builder.WriteOpeningBracket();
            var typeArgs = types.Select(m => $"typeof({m})").JoinWith(", ");
            builder.WriteLine($"return type.MakeGenericType({typeArgs});");
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