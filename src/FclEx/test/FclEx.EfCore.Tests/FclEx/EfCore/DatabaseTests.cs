namespace FclEx.EfCore;

[Collection(nameof(DatabaseTests))]
public class DatabaseTests
{
    public static readonly IEnumerable<object?[]> DbTestCases = DatabaseTypes
        .Select(m => new object[] { m });

    public static readonly IEnumerable<object?[]> SchemaCases = Schemas.Select(m => new object?[] { m });

    public static readonly IEnumerable<object?[]> DbSchemaTestCases = DatabaseTypes
        .SelectMany(m => Schemas, (x, y) => (x, y))
        .Select(m => new object?[] { m.x, m.y });
}