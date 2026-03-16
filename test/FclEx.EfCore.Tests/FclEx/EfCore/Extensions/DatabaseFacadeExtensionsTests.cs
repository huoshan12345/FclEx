namespace FclEx.EfCore.Extensions;

public class DatabaseFacadeExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_ReturnsValue(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_ReturnsNull_WhenSqlReturnsNull(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<int?>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    // ReSharper disable once InconsistentNaming
    public async Task ExecuteScalarRawAsync_ReturnsDefault_WhenDBNull(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<string>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_SupportsParameters(DbProviderType dbProviderType)
    {
        var parameter = CreateParameter(dbProviderType, "@p0", 5);
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT @p0 + 1", [parameter]);
        Assert.Equal(6, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_ConvertsResultType(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT 1");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_Works_WhenConnectionAlreadyOpen(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        await context.Database.OpenConnectionAsync();

        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_Works_WithEmptyParameters(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1", []);

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_NumericWidening(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT COUNT(*)");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteScalarRawAsync_Guid(DbProviderType dbProviderType)
    {
        var expected = Guid.NewGuid();
        var parameter = CreateParameter(dbProviderType, "@p0", expected);

        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.Database.ExecuteScalarRawAsync<Guid>("SELECT @p0", [parameter]);

        Assert.Equal(expected, result);
    }
}
