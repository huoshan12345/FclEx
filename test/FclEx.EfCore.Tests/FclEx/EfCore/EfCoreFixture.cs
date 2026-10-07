namespace FclEx.EfCore;

public class EfCoreFixture : CoreTestsFixture
{
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;
    internal static readonly string?[] Schemas = SchemaNames
        .Select(schema => WithAssemblyInfo(schema, typeof(EfCoreFixture).Assembly)).ToArray();

    public TestDatabaseEnvironment Environment { get; }
    public DatabaseUser DefaultUser => Environment.DefaultUser;

    public EfCoreFixture()
    {
        Environment = new(Databases, WithAssemblyInfo(DbName),
            new(WithAssemblyInfo(DefaultUserName), DefaultUserPassword, WithAssemblyInfo(UserSchema)));
    }

    public TestDatabaseTarget ResolveTarget(DbDriver driver, TestLogin login = TestLogin.Standard, string? schema = null)
        => Environment.Resolve(driver, schema, login);

    public TestDbContext CreateDbContext(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
        => new(driver, ResolveTarget(driver, login, schema).BuildConnectionString(), schema);

    public override async ValueTask InitializeAsync()
    {
        if (DbDrivers.Contains(DbDriver.Sqlite))
            await Environment.InitializeSqliteAsync();

        foreach (var (driver, schema) in DatabaseTestSettings.SchemaCases(Schemas))
        {
            await using var connection = ResolveTarget(driver, schema: schema).CreateConnection();
            await FixAutoIncrement<EntityWithAutoKey>(connection, driver, schema);
        }
    }

    public override ValueTask DisposeAsync()
    {
        Environment.Dispose();
        return base.DisposeAsync();
    }
}
