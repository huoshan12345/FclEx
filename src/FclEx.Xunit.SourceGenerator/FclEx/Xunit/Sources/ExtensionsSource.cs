namespace FclEx.Xunit.Sources;

internal class ExtensionsSource
{
    private const int Max = 8;

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Xunit";
        const string className = "Extensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"partial class {className}")
            .WriteOpeningBracket();

        /*
            public static TheoryData<T1, T2> ToTheoryData<T1, T2>(this IEnumerable<(T1, T2)> enumerable)
            {
                var data = new TheoryData<T1, T2>();
                foreach (var (item1, item2) in enumerable)
                {
                    data.Add(item1, item2);
                }
                return data;
            }
         */
        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"T{m}").JoinWith(", ");
            builder.WriteLine($"public static TheoryData<{types}> ToTheoryData<{types}>(this IEnumerable<({types})> enumerable)");
            builder.WriteOpeningBracket();

            var items = Enumerable.Range(1, i).Select(m => $"item{m}").JoinWith(", ");
            var body = $$"""
                         var data = new TheoryData<{{types}}>();
                         foreach (var ({{items}}) in enumerable)
                         {
                             data.Add({{items}});
                         }
                         return data;
                         """;
            builder.WriteLines(body);

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