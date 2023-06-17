using FclEx.Xunit;

namespace FclEx.Dapper;

public class GlobalFixture : IAsyncLifetime
{
    public static readonly string[] Schemas = { "schema_test_1", "schema_test_2" };

    public static readonly DatabaseType[] DatabaseTypes = LocalTestHelper.IsGithubAction
        ? new[] { DatabaseType.Npgsql }
        : Enum.GetValues<DatabaseType>();

    // InitializeAsync is called immediately after the class has been created, before it is used.
    // We use this method to initialize database only once before all tests.
    public async Task InitializeAsync()
    {
        foreach (var databaseType in DatabaseTypes)
        {
            var isRecreated = false; // NOTE: we delete database only once for every database instance.
            foreach (var schema in Schemas)
            {
                await using var context = GlobalDbContext.Create(databaseType, schema);

                try
                {

                    if (databaseType is DatabaseType.MySqlConnector && schema.IsValid())
                    {
                        await context.Database.ExecuteSqlRawAsync($"DROP DATABASE IF EXISTS `{schema}`;");
                        await context.Database.ExecuteSqlRawAsync($"CREATE DATABASE `{schema}`;");
                    }
                    else
                    {
                        if (isRecreated == false)
                        {
                            await context.Database.EnsureDeletedAsync();
                            await context.Database.EnsureCreatedAsync();
                        }
                        isRecreated = true;
                    }

                    var databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
                    await databaseCreator.CreateTablesAsync();
                }
                catch (SqlException ex) when (ex.Message.Contains("already an object named"))
                {
                }
                catch (Exception ex) when (ex.Message.Contains("already exists"))
                {
                }
            }
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}