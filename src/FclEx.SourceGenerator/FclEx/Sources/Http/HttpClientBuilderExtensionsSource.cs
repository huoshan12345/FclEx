namespace FclEx.Sources.Http;

internal class HttpClientBuilderExtensionsSource
{
    private const int Max = 8;

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.Http";
        const string className = "HttpClientBuilderExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace, true);

        // Class declaration
        builder.WriteLine($"public partial class {className}")
            .WriteOpeningBracket();

        var methodNames = new[] { "AddHttpMessageHandler", "ConfigurePrimaryHttpMessageHandler" };

        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"TDependency{m}").JoinWith(", ");

            foreach (var methodName in methodNames)
            {
                // NOTE: there is a "By" suffix.
                builder.WriteLine($"public static IHttpClientBuilder {methodName}By<{types}>(this IHttpClientBuilder builder, Func<{types}, DelegatingHandler> func)");

                builder.Indent();
                foreach (var j in Enumerable.Range(1, i))
                {
                    builder.WriteLine($"where TDependency{j} : notnull");
                }
                builder.Unindent();

                builder.WriteOpeningBracket();

                builder.WriteLine($"builder.{methodName}(s => ");
                builder.WriteOpeningBracket();

                var variables = new List<string>();
                foreach (var j in Enumerable.Range(1, i))
                {
                    var variable = $"service{j}";
                    builder.WriteLine($"var {variable} = s.GetRequiredService<TDependency{j}>();");
                    variables.Add(variable);
                }
                builder.WriteLine($"return func({variables.JoinWith(", ")});");

                builder.Unindent();
                builder.WriteLine("});");

                builder.WriteLine("return builder;");

                builder.WriteClosingBracket();
                builder.WriteLine();
            }
        }

        // End class declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
