using Dapper;

namespace FclEx.Dapper;

public class TypeHandlerTests
{
    [Theory]
    [InlineData(null)]
    [MemberData(nameof(DbNullValues))]
    public void GuidTypeHandler_NullDatabaseValue_Throws(object? value)
    {
        var handler = new GuidTypeHandler();

        Assert.Throws<InvalidCastException>(() => handler.Parse(value!));
    }

    public static TheoryData<object?> DbNullValues => new() { DBNull.Value };

    [Fact]
    public void AssumeUtcDateTimeTypeHandler_LocalValue_ConvertsToUtc()
    {
        var handler = new AssumeUtcDateTimeTypeHandler();
        var local = new DateTime(2026, 8, 27, 12, 0, 0, DateTimeKind.Local);

        var parsed = handler.Parse(local);
        var parameter = new SqliteParameter();
        handler.SetValue(parameter, local);

        Assert.Equal(local.ToUniversalTime(), parsed);
        Assert.Equal(DateTimeKind.Utc, parsed.Kind);
        Assert.Equal(local.ToUniversalTime(), parameter.Value);
    }
}
