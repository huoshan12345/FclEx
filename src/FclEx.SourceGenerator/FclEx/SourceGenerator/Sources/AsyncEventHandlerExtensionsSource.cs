namespace FclEx.SourceGenerator.Sources;

internal static class AsyncEventHandlerExtensionsSource
{
    private const int Max = 8;
    private static readonly string[] _usings =
    {
        "FclEx",
    };

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "AsyncEventHandlerExtensions";
        const string methodName = "public static Task InvokeAsync";

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
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").Prepend("TSender").JoinWith(", ");
            var @params = Enumerable.Range(1, i).Select(m => $"T{m} arg{m}").Prepend("TSender sender").JoinWith(", ");
            var args = Enumerable.Range(1, i).Select(m => $"arg{m}").Prepend("sender").JoinWith(", ");
            var handlerType = $"AsyncEventHandler<{types}>";
            builder.WriteLine($"{methodName}<{types}>(this {handlerType} handler, {@params})");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return handler.GetInvocationList<{handlerType}>().Select(m => m({args})).WhenAll();");
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