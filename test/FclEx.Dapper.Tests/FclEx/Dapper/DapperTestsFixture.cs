namespace FclEx.Dapper;

public class DapperTestsFixture : CoreTestsFixture
{
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;
    internal static readonly string?[] Schemas = SchemaNames
        .Select(schema => WithAssemblyInfo(schema, typeof(DapperTestsFixture).Assembly)).ToArray();

    public TestDatabaseEnvironment DatabaseEnvironment { get; }

    public DapperTestsFixture()
    {
        DatabaseEnvironment = new(Databases, WithAssemblyInfo(DatabaseName),
            new(WithAssemblyInfo(DefaultUserName), DefaultUserPassword, WithAssemblyInfo(UserSchema)));
    }

    public DbConnection CreateDbConnection(DbDriver driver, string? schema, TestLogin login = TestLogin.Standard)
        => DatabaseEnvironment.Resolve(driver, schema, login).CreateConnection();

    public override async ValueTask InitializeAsync()
    {
        if (SelectedDrivers.Contains(DbDriver.Sqlite))
            await DatabaseEnvironment.InitializeSqliteAsync();

        foreach (var (driver, schema) in GetDriverSchemaCases(Schemas))
        {
            // ReSharper disable once UseAwaitUsing
            using var connection = CreateDbConnection(driver, schema);
            await SynchronizeIdentitySequenceAsync<EntityWithAutoKey>(connection, driver, schema);
        }
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        DatabaseEnvironment.Dispose();
        return base.DisposeAsync();
    }
}
