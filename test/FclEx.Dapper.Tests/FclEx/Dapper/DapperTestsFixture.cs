namespace FclEx.Dapper;

public class DapperTestsFixture : CoreTestsFixture
{
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;
    internal static readonly string?[] Schemas = SchemaNames
        .Select(schema => WithAssemblyInfo(schema, typeof(DapperTestsFixture).Assembly)).ToArray();

    public TestDatabaseEnvironment Environment { get; }

    public DapperTestsFixture()
    {
        Environment = new(Databases, WithAssemblyInfo(DbName),
            new(WithAssemblyInfo(DefaultUserName), DefaultUserPassword, WithAssemblyInfo(UserSchema)));
    }

    public DbConnection CreateDbConnection(DbDriver driver, string? schema, TestLogin login = TestLogin.Standard)
        => Environment.Resolve(driver, schema, login).CreateConnection();

    public override async ValueTask InitializeAsync()
    {
        if (DbDrivers.Contains(DbDriver.Sqlite))
            await Environment.InitializeSqliteAsync();

        foreach (var (driver, schema) in DatabaseTestSettings.SchemaCases(Schemas))
        {
            using var connection = CreateDbConnection(driver, schema);
            await FixAutoIncrement<EntityWithAutoKey>(connection, driver, schema);
        }
    }

    public override ValueTask DisposeAsync()
    {
        Environment.Dispose();
        return base.DisposeAsync();
    }
}
