// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public class TypeHandlerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-5)]
    public async Task Initialize_SqliteDateTimeOffset_RoundTripsValueAndOffset(int offsetHours)
    {
        DapperHelper.Initialize();
        using var connection = new SqliteConnection("Data Source=:memory:");
        var expected = new DateTimeOffset(2026, 10, 7, 12, 34, 56, TimeSpan.FromHours(offsetHours)).AddTicks(1234567);

        var actual = await connection.QuerySingleAsync<OffsetRow>("SELECT @Value AS Value", new { Value = expected });

        Assert.Equal(expected.Ticks, actual.Value.Ticks);
        Assert.Equal(expected.Offset, actual.Value.Offset);
    }

    private sealed class OffsetRow
    {
        public DateTimeOffset Value { get; set; }
    }

    [Theory]
    [InlineData(null)]
    [MemberData(nameof(DbNullValues))]
    public void DateTimeOffsetTypeHandler_NullDatabaseValue_Throws(object? value)
        => Assert.Throws<InvalidCastException>(() => new DateTimeOffsetTypeHandler().Parse(value!));

    [Theory]
    [InlineData(DateTimeKind.Unspecified)]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    public void DateTimeOffsetTypeHandler_DateTime_PreservesInstantWithExplicitUnspecifiedPolicy(DateTimeKind kind)
    {
        var value = new DateTime(2026, 10, 7, 12, 34, 56, kind);
        var expected = new DateTimeOffset(kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value);
        Assert.Equal(expected, new DateTimeOffsetTypeHandler().Parse(value));
    }

    [Fact]
    public async Task Initialize_SqliteNullableDateTimeOffset_PreservesNull()
    {
        DapperHelper.Initialize();
        using var connection = new SqliteConnection("Data Source=:memory:");
        var value = await connection.QuerySingleAsync<DateTimeOffset?>("SELECT @value", new { value = (DateTimeOffset?)null });
        Assert.Null(value);
    }

    [Fact]
    public async Task Initialize_SqliteNullableDateTimeOffset_PreservesValue()
    {
        DapperHelper.Initialize();
        using var connection = new SqliteConnection("Data Source=:memory:");
        DateTimeOffset? expected = new DateTimeOffset(2026, 10, 7, 12, 34, 56, TimeSpan.FromHours(8));

        var actual = await connection.QuerySingleAsync<DateTimeOffset?>("SELECT @value", new { value = expected });

        Assert.Equal(expected!.Value.Ticks, actual!.Value.Ticks);
        Assert.Equal(expected.Value.Offset, actual.Value.Offset);
    }

    [Theory]
    [InlineData("2026-10-07 12:34:56.1234567", 0)]
    [InlineData("2026-10-07 12:34:56.1234567+08:00", 8)]
    [InlineData("2026-10-07T12:34:56.1234567-05:00", -5)]
    public void DateTimeOffsetTypeHandler_Text_UsesInvariantCultureAndPreservesOffset(string text, int offsetHours)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var expected = new DateTimeOffset(2026, 10, 7, 12, 34, 56, TimeSpan.FromHours(offsetHours)).AddTicks(1234567);
            var actual = new DateTimeOffsetTypeHandler().Parse(text);

            Assert.Equal(expected.Ticks, actual.Ticks);
            Assert.Equal(expected.Offset, actual.Offset);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void DateTimeOffsetTypeHandler_NativeValue_PreservesOffsetAndPrecision()
    {
        var expected = new DateTimeOffset(2026, 10, 7, 12, 34, 56, TimeSpan.FromHours(8)).AddTicks(1234567);
        var actual = new DateTimeOffsetTypeHandler().Parse(expected);

        Assert.Equal(expected.Ticks, actual.Ticks);
        Assert.Equal(expected.Offset, actual.Offset);
    }

    [Fact]
    public void DateTimeOffsetTypeHandler_InvalidValue_Throws()
    {
        var handler = new DateTimeOffsetTypeHandler();
        Assert.Throws<FormatException>(() => handler.Parse("invalid"));
        Assert.Throws<InvalidCastException>(() => handler.Parse(123));
    }

    [Fact(DisableParallelization = true)]
    public async Task Initialize_CustomGuidHandler_DoesNotPreventDateTimeOffsetRegistration()
    {
        SqlMapper.ResetTypeHandlers();
        SqlMapper.AddTypeHandler(new CustomGuidHandler());
        try
        {
            DapperHelper.Initialize();
            using var connection = new SqliteConnection("Data Source=:memory:");
            Assert.Equal(CustomGuidHandler.Value, await connection.QuerySingleAsync<Guid>("SELECT 'custom'"));
            Assert.True(SqlMapper.HasTypeHandler(typeof(DateTimeOffset)));
        }
        finally
        {
            SqlMapper.ResetTypeHandlers();
            DapperHelper.Initialize();
        }
    }

    [Fact(DisableParallelization = true)]
    public async Task Initialize_CustomDateTimeOffsetHandler_IsPreserved()
    {
        SqlMapper.ResetTypeHandlers();
        SqlMapper.AddTypeHandler(new CustomOffsetHandler());
        try
        {
            DapperHelper.Initialize();
            using var connection = new SqliteConnection("Data Source=:memory:");
            Assert.Equal(CustomOffsetHandler.Value, await connection.QuerySingleAsync<DateTimeOffset>("SELECT 'custom'"));
            Assert.True(SqlMapper.HasTypeHandler(typeof(Guid)));
        }
        finally
        {
            SqlMapper.ResetTypeHandlers();
            DapperHelper.Initialize();
        }
    }

    private sealed class CustomGuidHandler : SqlMapper.TypeHandler<Guid>
    {
        public static readonly Guid Value = Guid.NewGuid();
        public override Guid Parse(object value) => Value;
        public override void SetValue(IDbDataParameter parameter, Guid value) => parameter.Value = value;
    }

    private sealed class CustomOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public static readonly DateTimeOffset Value = DateTimeOffset.UtcNow;
        public override DateTimeOffset Parse(object value) => Value;
        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) => parameter.Value = value;
    }

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
