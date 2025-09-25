using System.Collections.Generic;

namespace FclEx.Sources.DependencyInjection;

internal class ServiceCollectionExtensionsSource
{
    private const int Max = 8;

    internal static SourceInfo Generate()
    {
        const string @namespace = "FclEx.DependencyInjection";
        const string className = "ServiceCollectionExtensions";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        // Class declaration
        builder.WriteLine($"public partial class {className}")
            .WriteOpeningBracket();

        /*
           public static IServiceCollection AddSingletonBy<T, TDependency1, TDependency2>(this IServiceCollection services, Func<TDependency1, TDependency2, T> func)
               where T : class
               where TDependency1 : notnull
               where TDependency2 : notnull
           {
               services.AddSingleton(s => 
               {
                   var service1 = s.GetRequiredService<TDependency1>();
                   var service2 = s.GetRequiredService<TDependency2>();
                   return func(service1, service2);
               });
               return services;
           }
         */
        var methodNames = new[] { "Add", "TryAdd" }
            .SelectMany(m => new[] { "Singleton", "Scoped", "Transient" }, (p, m) => p + m)
            .ToArray();

        for (var i = 2; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"TDependency{m}").JoinWith(", ");

            foreach (var methodName in methodNames)
            {
                builder.WriteLine($"public static IServiceCollection {methodName}<T, {types}>(this IServiceCollection services, Func<{types}, T> func)");

                builder.Indent();
                builder.WriteLine("where T : class");
                foreach (var j in Enumerable.Range(1, i))
                {
                    builder.WriteLine($"where TDependency{j} : notnull");
                }
                builder.Unindent();

                builder.WriteOpeningBracket();

                builder.WriteLine($"services.{methodName}(s => ");
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

                builder.WriteLine("return services;");

                builder.WriteClosingBracket();
                builder.WriteLine();
            }
        }

        // End class declaration
        builder.WriteClosingBracket();

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ($"{className}.g.cs", str);
    }
}
