namespace FclEx.Sources;

internal static class OperationResultExtensionsSource
{
    private static readonly string[] _usings =
    [
        "System.Security.Cryptography",
        "FclEx.Extensions",
    ];

    private static readonly string[] _types =
    [
        nameof(IEnumerable<>),
        nameof(ICollection<>),
        nameof(IReadOnlyCollection<>),
        nameof(IList<>),
        nameof(IReadOnlyList<>),
        nameof(List<>),
    ];

    public static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Extensions";
        const string className = "OperationResultExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteUsings(_usings)
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace, true)
            .WriteLine();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        /*
            public static Task<OperationResult<T[]>> Merge<T>(this Task<OperationResult<T>[]> task)
            {
               return task.Merge<T, OperationResult<T>[]>();
            }

            public static IAction<T[]> ToAction<T>(this Task<OperationResult<T>[]> task)
            {
               return task.ToAction<T, OperationResult<T>[]>();
            }
         */

        foreach (var type in _types)
        {
            builder.WriteLine($"public static Task<OperationResult<T[]>> Merge<T>(this Task<{type}<OperationResult<T>>> task)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return task.Merge<T, {type}<OperationResult<T>>>();");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.WriteLine($"public static IAction<T[]> ToAction<T>(this Task<{type}<OperationResult<T>>> task)");
            builder.WriteOpeningBracket();
            builder.WriteLine($"return task.ToAction<T, {type}<OperationResult<T>>>();");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
