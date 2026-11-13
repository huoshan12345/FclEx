namespace FclEx.EfCore;

public class EfCoreFixture : CoreTestsFixture
{
    public static readonly DatabasesConfig Databases = Config.GetSection("Databases").Get<DatabasesConfig>()!;
    internal static readonly string?[] Schemas = SchemaNames
        .Select(schema => WithAssemblyInfo(schema, typeof(EfCoreFixture).Assembly)).ToArray();

    public TestDatabaseEnvironment DatabaseEnvironment { get; }
    public TruncateTestSessions TruncateSessions { get; }
    public DatabaseUser DefaultUser => DatabaseEnvironment.DefaultUser;

    public EfCoreFixture()
    {
        DatabaseEnvironment = new(Databases, WithAssemblyInfo(DatabaseName),
            new(WithAssemblyInfo(DefaultUserName), DefaultUserPassword, WithAssemblyInfo(UserSchema)));
        TruncateSessions = new(DatabaseEnvironment);
    }

    public TestDatabaseTarget ResolveTarget(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
        => DatabaseEnvironment.Resolve(driver, schema, login);

    public TestDbContext CreateDbContext(DbDriver driver, string? schema = null, TestLogin login = TestLogin.Standard)
        => new(driver, ResolveTarget(driver, schema, login).BuildConnectionString(), schema);

    public override async ValueTask InitializeAsync()
    {
        await DatabaseTestSettings.InitializeAsync(DatabaseEnvironment, Schemas);
    }

    public override ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        DatabaseEnvironment.Dispose();
        return base.DisposeAsync();
    }
}
