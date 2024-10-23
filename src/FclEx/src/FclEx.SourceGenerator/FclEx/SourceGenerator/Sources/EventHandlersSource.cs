namespace FclEx.SourceGenerator.Sources;

internal class EventHandlersSource
{
    private const int Max = 8;

    internal static (string FileName, string Code) Generate()
    {
        const string @namespace = "FclEx";

        using var builder = new SourceBuilder()
            .WriteGeneratedHeader()
            .WriteLine()
            .WriteLine();

        // Namespace declaration
        builder.WriteNamespace(@namespace)
            .WriteOpeningBracket();

        for (var i = 1; i <= Max; i++)
        {
            var types = Enumerable.Range(1, i).Select(m => $"in T{m}").JoinWith(", ");
            var @params = Enumerable.Range(1, i).Select(m => $"T{m} arg{m}").JoinWith(", ");

            builder.WriteLine($"public delegate void EventHandler<in TSender, {types}>(TSender sender, {@params});");
            builder.WriteLine();
            builder.WriteLine($"public delegate Task AsyncEventHandler<in TSender, {types}>(TSender sender, {@params});");
            builder.WriteLine();
        }

        // End namespace declaration
        builder.WriteClosingBracket();

        var str = builder.ToString();
        return ("EventHandlers.g.cs", str);
    }

}
