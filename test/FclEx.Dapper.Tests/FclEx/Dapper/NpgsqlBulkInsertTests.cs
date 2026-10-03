using Npgsql;

namespace FclEx.Dapper;

public class NpgsqlBulkInsertTests
{
    public static TheoryData<byte, ushort, uint, ulong, int> UnsignedValueCases => new()
    {
        { 0, 0, 0u, 0ul, 1 },
        { 128, 32768, (uint)int.MaxValue + 1u, (ulong)long.MaxValue + 1ul, 1 },
        { byte.MaxValue, ushort.MaxValue, uint.MaxValue, ulong.MaxValue, 1 },
        { 0, 0, 0u, 0ul, 501 },
        { 128, 32768, (uint)int.MaxValue + 1u, (ulong)long.MaxValue + 1ul, 501 },
        { byte.MaxValue, ushort.MaxValue, uint.MaxValue, ulong.MaxValue, 501 },
    };

    [Theory]
    [InlineData(0u)]
    [InlineData((uint)int.MaxValue + 1u)]
    [InlineData(uint.MaxValue)]
    public async Task NpgsqlCommand_UInt32ParameterWithoutStoreType_ThrowsInvalidCastException(uint value)
    {
        Assert.SkipUnlessIncluded(DbDriver.Npgsql);

        using var connection = new DapperTestsFixture().CreateDbConnection(DbDriver.Npgsql, null);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT @value";
        command.Parameters.Add(new NpgsqlParameter("value", value));

        var exception = await Assert.ThrowsAsync<InvalidCastException>(() => command.ExecuteScalarAsync());

        Assert.Contains("System.UInt32", exception.Message);
        Assert.Contains("NpgsqlDbType or DataTypeName", exception.Message);
        Assert.Equal(ConnectionState.Open, connection.State);
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT 1"));
    }

    [Theory]
    [MemberData(nameof(UnsignedValueCases))]
    public async Task BulkInsertAsync_UnsignedProperties_PersistsFullRanges(
        byte byteValue, ushort uint16Value, uint uint32Value, ulong uint64Value, int count)
    {
        Assert.SkipUnlessIncluded(DbDriver.Npgsql);

        // Only PostgreSQL is needed; temporary tables are isolated per connection.
        using var connection = new DapperTestsFixture().CreateDbConnection(DbDriver.Npgsql, null);
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            """
            CREATE TEMP TABLE bulk_insert_unsigned_rows (
                "ByteValue" smallint NOT NULL,
                "UInt16Value" integer NOT NULL,
                "VolumeSerialNumber" bigint NOT NULL,
                "UInt64Value" numeric(20,0) NOT NULL,
                "OptionalUInt16Value" integer,
                "OptionalUInt32Value" bigint,
                "OptionalUInt64Value" numeric(20,0));
            """);
        var entities = Enumerable.Range(0, count)
            .Select(index => new UnsignedRow
            {
                ByteValue = byteValue,
                UInt16Value = uint16Value,
                VolumeSerialNumber = uint32Value,
                UInt64Value = uint64Value,
                OptionalUInt16Value = index % 2 == 0 ? null : uint16Value,
                OptionalUInt32Value = index % 2 == 0 ? null : uint32Value,
                OptionalUInt64Value = index % 2 == 0 ? null : uint64Value,
            })
            .ToArray();

        var affectedRows = await connection.BulkInsertAsync(entities, includeAutoKey: true);

        Assert.Equal(count, affectedRows);
        var storedRows = (await connection.QueryAsync<StoredRow>("SELECT * FROM bulk_insert_unsigned_rows")).ToArray();
        Assert.Equal(count, storedRows.Length);
        Assert.All(storedRows, row =>
        {
            Assert.Equal((short)byteValue, row.ByteValue);
            Assert.Equal((int)uint16Value, row.UInt16Value);
            Assert.Equal((long)uint32Value, row.VolumeSerialNumber);
            Assert.Equal((decimal)uint64Value, row.UInt64Value);
        });
        var populatedRows = storedRows.Where(row => row.OptionalUInt32Value.HasValue).ToArray();
        Assert.Equal(count / 2, populatedRows.Length);
        Assert.All(populatedRows, row =>
        {
            Assert.Equal((int?)uint16Value, row.OptionalUInt16Value);
            Assert.Equal((long?)uint32Value, row.OptionalUInt32Value);
            Assert.Equal((decimal?)uint64Value, row.OptionalUInt64Value);
        });
        Assert.All(storedRows.Except(populatedRows), row =>
        {
            Assert.Null(row.OptionalUInt16Value);
            Assert.Null(row.OptionalUInt32Value);
            Assert.Null(row.OptionalUInt64Value);
        });
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Table("bulk_insert_unsigned_rows")]
    private sealed class UnsignedRow
    {
        public byte ByteValue { get; set; }
        public ushort UInt16Value { get; set; }
        public uint VolumeSerialNumber { get; set; }
        public ulong UInt64Value { get; set; }
        public ushort? OptionalUInt16Value { get; set; }
        public uint? OptionalUInt32Value { get; set; }
        public ulong? OptionalUInt64Value { get; set; }
    }

    private sealed class StoredRow
    {
        public short ByteValue { get; set; }
        public int UInt16Value { get; set; }
        public long VolumeSerialNumber { get; set; }
        public decimal UInt64Value { get; set; }
        public int? OptionalUInt16Value { get; set; }
        public long? OptionalUInt32Value { get; set; }
        public decimal? OptionalUInt64Value { get; set; }
    }
}
