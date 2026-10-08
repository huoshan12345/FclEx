namespace FclEx.EfCore;

public class SqliteSchemaTests
{
    [Fact]
    public void GeneratedSchema_MatchesSharedResource()
    {
        using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        Assert.Equal(Normalize(context.Database.GenerateCreateScript()), Normalize(TestDatabaseEnvironment.ReadSqliteSchema()));
    }

#if NET10_0
    [Fact(Explicit = true)]
    public void ExportSchema()
    {
        var path = System.Environment.GetEnvironmentVariable("FCLEX_SQLITE_SCHEMA_OUTPUT")
            ?? throw new InvalidOperationException("Set FCLEX_SQLITE_SCHEMA_OUTPUT to test/FclEx.DatabaseTesting/Schemas/Sqlite.sql.");
        using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        var sql = Normalize(context.Database.GenerateCreateScript()).Replace("\n", System.Environment.NewLine);
        File.WriteAllText(path, sql + System.Environment.NewLine, new UTF8Encoding(false));
    }
#endif

    private static string Normalize(string sql) => sql.Replace("\r\n", "\n").TrimEnd();
}
