using Npgsql;
using NpgsqlTypes;

namespace FclEx.Dapper;

public class NpgsqlAdapterTests
{
    public static TheoryData<object, object, NpgsqlDbType> UnsignedParameterCases => new()
    {
        { (ushort)0, 0, NpgsqlDbType.Integer },
        { (ushort)32768, 32768, NpgsqlDbType.Integer },
        { ushort.MaxValue, (int)ushort.MaxValue, NpgsqlDbType.Integer },
        { 0u, 0L, NpgsqlDbType.Bigint },
        { (uint)int.MaxValue + 1u, (long)int.MaxValue + 1L, NpgsqlDbType.Bigint },
        { uint.MaxValue, (long)uint.MaxValue, NpgsqlDbType.Bigint },
        { 0ul, 0m, NpgsqlDbType.Numeric },
        { (ulong)long.MaxValue + 1ul, (decimal)long.MaxValue + 1m, NpgsqlDbType.Numeric },
        { ulong.MaxValue, (decimal)ulong.MaxValue, NpgsqlDbType.Numeric },
    };

    [Theory]
    [MemberData(nameof(UnsignedParameterCases))]
    public async Task CreateParameter_UnsignedInteger_UsesLosslessProviderValue(
        object value, object expectedValue, NpgsqlDbType expectedType)
    {
        Assert.SkipUnlessIncluded(DbDriver.Npgsql);

        var parameter = Assert.IsType<NpgsqlParameter>(new NpgsqlAdapter().CreateParameter("@value", value));

        Assert.Equal("@value", parameter.ParameterName);
        Assert.IsType(expectedValue.GetType(), parameter.Value);
        Assert.Equal(expectedValue, parameter.Value);
        // Type inference requires the provider's data source; verify the actual type PostgreSQL receives.
        using var connection = new DapperTestsFixture().CreateDbConnection(DbDriver.Npgsql, null);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT pg_typeof(@value)::text";
        command.Parameters.Add(parameter);
        Assert.Equal(expectedType.ToString().ToLowerInvariant(), await command.ExecuteScalarAsync());
    }

    [Theory]
    [InlineData("Integer", NpgsqlDbType.Integer)]
    [InlineData("Bigint", NpgsqlDbType.Bigint)]
    [InlineData("Numeric", NpgsqlDbType.Numeric)]
    public void CreateParameter_ExplicitNumericType_PreservesRequestedType(string storeType, NpgsqlDbType expectedType)
    {
        var parameter = Assert.IsType<NpgsqlParameter>(new NpgsqlAdapter().CreateParameter("value", 1u, storeType));

        Assert.Equal(1L, parameter.Value);
        Assert.Equal(expectedType, parameter.NpgsqlDbType);
    }

    [Theory]
    [InlineData("Oid", NpgsqlDbType.Oid)]
    [InlineData("Xid", NpgsqlDbType.Xid)]
    [InlineData("Cid", NpgsqlDbType.Cid)]
    [InlineData("Regtype", NpgsqlDbType.Regtype)]
    [InlineData("Regconfig", NpgsqlDbType.Regconfig)]
    [InlineData(" oID ", NpgsqlDbType.Oid)]
    public void CreateParameter_ExplicitUnsignedType_PreservesUInt32(string storeType, NpgsqlDbType expectedType)
    {
        var parameter = Assert.IsType<NpgsqlParameter>(new NpgsqlAdapter().CreateParameter("value", uint.MaxValue, storeType));

        Assert.IsType<uint>(parameter.Value);
        Assert.Equal(uint.MaxValue, parameter.Value);
        Assert.Equal(expectedType, parameter.NpgsqlDbType);
    }

    [Fact]
    public void CreateParameter_ExplicitXid8_PreservesUInt64()
    {
        var parameter = Assert.IsType<NpgsqlParameter>(new NpgsqlAdapter().CreateParameter("value", ulong.MaxValue, "Xid8"));

        Assert.IsType<ulong>(parameter.Value);
        Assert.Equal(ulong.MaxValue, parameter.Value);
        Assert.Equal(NpgsqlDbType.Xid8, parameter.NpgsqlDbType);
    }

    public static TheoryData<object?> UnchangedValueCases => new()
    {
        { (object?)null },
        DBNull.Value,
        byte.MaxValue,
        short.MinValue,
        int.MinValue,
        long.MinValue,
        decimal.MaxValue,
        "text",
        new byte[] { 0, 255 },
    };

    [Theory]
    [MemberData(nameof(UnchangedValueCases))]
    public void CreateParameter_OtherValues_RemainUnchanged(object? value)
    {
        var parameter = new NpgsqlAdapter().CreateParameter("value", value);

        Assert.Same(value ?? DBNull.Value, parameter.Value);
    }
}
