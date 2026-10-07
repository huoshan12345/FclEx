using FclEx.Dapper;

namespace FclEx.Databases;

public class TestDatabaseEnvironmentTests
{
    public static TheoryData<DbDriver, TestLogin, string?> TargetCases =>
        (from driver in SupportedDrivers.Where(driver => driver != DbDriver.Sqlite)
         from login in new[] { TestLogin.Standard, TestLogin.DefaultSchemaUser }
         from schema in new string?[] { null, "tenant" }
         select (driver, login, schema)).ToTheoryData();

    private static DatabasesConfig CreateConfiguration(string password = "admin-password")
    {
        var config = new DatabaseConfig
        {
            Host = "localhost",
            Port = 1234,
            UserName = "admin",
            Password = password,
            ServiceName = "service",
        };
        return new()
        {
            SqlServer = config,
            Postgres = config,
            MySql = config,
            Oracle = config,
        };
    }

    private static TestDatabaseEnvironment CreateEnvironment(string password = "admin-password")
        => new(CreateConfiguration(password), "database", new("test_user", "test-password", "user_schema"));

    [Theory]
    [MemberData(nameof(TargetCases))]
    public void Resolve_KeepsDatabaseOwnerAndLoginConceptsDistinct(DbDriver driver, TestLogin login, string? schema)
    {
        using var environment = CreateEnvironment();
        var target = environment.Resolve(driver, schema, login);
        Assert.Equal(driver, target.Driver);
        Assert.Equal(schema, target.Schema);

        var expectedUser = login == TestLogin.Standard ? "admin" : "test_user";
        if (driver == DbDriver.Oracle)
        {
            Assert.Null(target.Database);
            expectedUser = login == TestLogin.Standard ? schema ?? "database" : "test_user";
        }
        else
            Assert.Equal(driver.IsMySql() ? schema ?? "database" : "database", target.Database);

        Assert.Equal(expectedUser, target.Login.UserName);
        Assert.Equal(login == TestLogin.Standard ? "admin-password"
            : driver == DbDriver.SqlServer ? SqlServerUserPassword : "test-password", target.Login.Password);
        using var connection = target.CreateConnection();
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public void Resolve_DifferentConfigurationsNeverShareCredentials()
    {
        using var first = CreateEnvironment("first");
        using var second = CreateEnvironment("second");
        Assert.Equal("first", first.Resolve(DbDriver.SqlServer).Login.Password);
        Assert.Equal("second", second.Resolve(DbDriver.SqlServer).Login.Password);
    }

    [Fact]
    public async Task SqliteEnvironments_AreIndependentAndCleanUpOwnedFiles()
    {
        using var first = CreateEnvironment();
        using var second = CreateEnvironment();
        await first.InitializeSqliteAsync();
        await second.InitializeSqliteAsync();
        var firstTarget = first.Resolve(DbDriver.Sqlite);
        var secondTarget = second.Resolve(DbDriver.Sqlite);
        Assert.NotEqual(firstTarget.FilePath, secondTarget.FilePath);
        Assert.True(Path.IsPathRooted(firstTarget.FilePath!));

        using (var connection = firstTarget.CreateConnection())
            await connection.InsertAsync<EntityWithAutoKey, int>(new() { Name = "first" });
        using (var connection = secondTarget.CreateConnection())
            Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EntityWithAutoKey"));

        first.Dispose();
        Assert.False(File.Exists(firstTarget.FilePath));
        Assert.True(File.Exists(secondTarget.FilePath));
        Assert.Throws<InvalidOperationException>(() => first.Resolve(DbDriver.Sqlite));
    }

    [Fact]
    public async Task SqliteSchema_EnforcesForeignKeysAndUniqueIndexes()
    {
        using var environment = CreateEnvironment();
        await environment.InitializeSqliteAsync();
        using var connection = environment.Resolve(DbDriver.Sqlite).CreateConnection();
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("PRAGMA foreign_keys"));
        var exception = await Assert.ThrowsAsync<SqliteException>(() => connection.ExecuteAsync(
            "INSERT INTO EntityWithNavigation (Name, NavigationId) VALUES ('invalid', -1)",
            cancellationToken: CancellationToken.None));
        Assert.Equal(19, exception.SqliteErrorCode);
        await connection.ExecuteAsync("INSERT INTO EntityWithIdAndIndex (Name, Value) VALUES ('unique', 1)", cancellationToken: CancellationToken.None);
        await Assert.ThrowsAsync<SqliteException>(() => connection.ExecuteAsync(
            "INSERT INTO EntityWithIdAndIndex (Name, Value) VALUES ('unique', 2)", cancellationToken: CancellationToken.None));
    }

    [Fact]
    public async Task SqliteEnvironment_ConcurrentConnectionsPreserveAllWrites()
    {
        using var environment = CreateEnvironment();
        await environment.InitializeSqliteAsync();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(worker => Task.Run(async () =>
        {
            using var connection = environment.Resolve(DbDriver.Sqlite).CreateConnection();
            for (var index = 0; index < 20; index++)
                await connection.InsertAsync<EntityWithAutoKey, int>(new() { Name = $"{worker}:{index}" });
        })));
        using var verification = environment.Resolve(DbDriver.Sqlite).CreateConnection();
        Assert.Equal(160, await verification.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM EntityWithAutoKey"));
    }
}
