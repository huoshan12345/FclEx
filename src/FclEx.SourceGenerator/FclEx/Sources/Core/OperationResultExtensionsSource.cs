using System.Reflection;

namespace FclEx.Sources.Core;

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

    private const int Max = 8;

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

        /*
            public static OperationResult<(T1, T2, TNext)> ThenWith<T1, T2, TNext>(
                this OperationResult<(T1, T2)> result,
                Func<T1, T2, OperationResult<TNext>> next)
            {
                Check.NotNull(next);
                return result.Then(m => next(m.Item1, m.Item2)
                    .MapValue(x => (m.Item1, m.Item2, x)));
            }

            public static OperationResult<(T1, T2, TNext)> ThenWith<T1, T2, TNext>(
                this OperationResult<(T1, T2)> result,
                Func<OperationResult<TNext>> next)
            {
                Check.NotNull(next);
                return result.Then(m => next()
                    .MapValue(x => (m.Item1, m.Item2, x)));
            }         
         */

        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => "T" + m).JoinWith(", ");
            var typesEx = types + ", TNext";
            var items = Enumerable.Range(1, i).Select(m => $"m.Item{m}").JoinWith(", ");

            builder.Write($"public static OperationResult<({typesEx})> ThenWith<{typesEx}>");
            builder.WriteLine($"(this OperationResult<({types})> result, Func<{types}, OperationResult<TNext>> next)");
            builder.WriteOpeningBracket();
            builder.WriteLine("Check.NotNull(next);");
            builder.WriteLine($"return result.Then(m => next({items}).MapValue(x => ({items}, x)));");
            builder.WriteClosingBracket();
            builder.WriteLine();

            builder.Write($"public static OperationResult<({typesEx})> ThenWith<{typesEx}>");
            builder.WriteLine($"(this OperationResult<({types})> result, Func<OperationResult<TNext>> next)");
            builder.WriteOpeningBracket();
            builder.WriteLine("Check.NotNull(next);");
            builder.WriteLine($"return result.Then(m => next().MapValue(x => ({items}, x)));");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        // End class declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
