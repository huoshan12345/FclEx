namespace FclEx.EfCore;

public class EfCoreFixture : CoreTestsFixture
{
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;
    internal static readonly string?[] Schemas = SchemaNames
        .Select(schema => WithAssemblyInfo(schema, typeof(EfCoreFixture).Assembly)).ToArray();

    public TestDatabaseEnvironment DatabaseEnvironment { get; }
    public TruncateTestTables TruncateTables { get; }
    public DatabaseUser DefaultUser => DatabaseEnvironment.DefaultUser;

    public EfCoreFixture()
    {
        DatabaseEnvironment = new(Databases, WithAssemblyInfo(DatabaseName),
            new(WithAssemblyInfo(DefaultUserName), DefaultUserPassword, WithAssemblyInfo(UserSchema)));
        TruncateTables = new(DatabaseEnvironment);
    }

    public TestDatabaseTarget ResolveTarget(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
        => DatabaseEnvironment.Resolve(driver, schema, login);

    public TestDbContext CreateDbContext(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
        => new(driver, ResolveTarget(driver, schema, login).BuildConnectionString(), schema);

    public override async ValueTask InitializeAsync()
    {
        if (SelectedDrivers.Contains(DbDriver.Sqlite))
            await DatabaseEnvironment.InitializeSqliteAsync();

        await TruncateTables.InitializeAsync(Schemas);

        foreach (var (driver, schema) in GetDriverSchemaCases(Schemas))
        {
            await using var connection = ResolveTarget(driver, schema: schema).CreateConnection();
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
