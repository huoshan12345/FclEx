namespace FclEx.Sources.Xunit;

internal class TheoryDataRowExtensionsSource
{
    private const int Max = 8;

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Xunit";
        const string className = "TheoryDataRowExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public static partial class {className}")
            .WriteOpeningBracket();

        /*
            public static TheoryData<T1, T2> ToTheoryData<T1, T2>(this IEnumerable<TheoryDataRow<T1, T2>> rows)
            {
                return new(rows);
            }
         */
        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");
            builder.WriteLine($"public static TheoryData<{types}> ToTheoryData<{types}>(this IEnumerable<TheoryDataRow<{types}>> rows)");
            builder.WriteOpeningBracket();
            builder.WriteLine("return new(rows);");
            builder.WriteClosingBracket();
            builder.WriteLine();
        }

        /*
            extension(TheoryDataRow)
            {
                public static TheoryDataRow<T1> New<T1>(T1 p1) => new(p1);
                public static TheoryDataRow<T1, T2> New<T1, T2>(T1 p1, T2 p2) => new(p1, p2);
            }
         */
        // extension declaration
        builder.WriteLine("extension(TheoryDataRow)");

        builder.WriteOpeningBracket();
        for (var i = 1; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");
            var parameters = Enumerable.Range(1, i).Select(m => $"T{m} p{m}").JoinWith(", ");
            var args = Enumerable.Range(1, i).Select(m => $"p{m}").JoinWith(", ");
            builder.WriteLine($"public static TheoryDataRow<{types}> New<{types}>({parameters}) => new({args});");
        }

        // End extension declaration
        builder.WriteClosingBracket();

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
