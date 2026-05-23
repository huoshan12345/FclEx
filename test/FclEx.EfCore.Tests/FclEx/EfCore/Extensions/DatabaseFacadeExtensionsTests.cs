namespace FclEx.EfCore.Extensions;

public class DatabaseFacadeExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ReturnsValue(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ReturnsNull_WhenSqlReturnsNull(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int?>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    // ReSharper disable once InconsistentNaming
    public async Task ExecuteScalarRawAsync_ReturnsDefault_WhenDBNull(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<string>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_SupportsParameters(DbDriver dbDriver)
    {
        var parameter = CreateParameter(dbDriver, "@p0", 5);
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT @p0 + 1", [parameter]);
        Assert.Equal(6, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ConvertsResultType(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT 1");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Works_WhenConnectionAlreadyOpen(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        await context.Database.OpenConnectionAsync();

        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Works_WithEmptyParameters(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1", []);

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_NumericWidening(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT COUNT(*)");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Guid(DbDriver dbDriver)
    {
        var expected = Guid.NewGuid();
        var parameter = CreateParameter(dbDriver, "@p0", expected);

        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<Guid>("SELECT @p0", [parameter]);

        Assert.Equal(expected, result);
    }
}
