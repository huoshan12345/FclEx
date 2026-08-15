namespace FclEx.Extensions;

public class DateTimeExtensionsTests
{
    public static TheoryData<DateTimeKind> DateTimeKinds => new()
    {
        DateTimeKind.Unspecified,
        DateTimeKind.Utc,
        DateTimeKind.Local,
    };

    [Theory]
    [MemberData(nameof(DateTimeKinds))]
    public void Calendar_Methods_Preserve_Kind_And_Accept_Milliseconds(DateTimeKind kind)
    {
        var dateTime = new DateTime(2024, 2, 14, 12, 34, 56, 789, kind);

        Assert.Equal(new DateTime(2024, 2, 14, 1, 2, 3, 4, kind), dateTime.Today(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 15, 1, 2, 3, 4, kind), dateTime.Tomorrow(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 13, 1, 2, 3, 4, kind), dateTime.Yesterday(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 3, 5, 1, 2, 3, 4, kind), dateTime.ThisYear(3, 5, 1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 5, 1, 2, 3, 4, kind), dateTime.ThisMonth(5, 1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 1, 1, 2, 3, 4, kind), dateTime.FirstDayOfMonth(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 29, 1, 2, 3, 4, kind), dateTime.LastDayOfMonth(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 12, 1, 2, 3, 4, kind), dateTime.FirstDayOfWeek(1, 2, 3, 4));
        Assert.Equal(new DateTime(2024, 2, 18, 1, 2, 3, 4, kind), dateTime.LastDayOfWeek(1, 2, 3, 4));

        Assert.All(
            new[]
            {
                dateTime.Today(),
                dateTime.Tomorrow(),
                dateTime.Yesterday(),
                dateTime.ThisYear(3, 5),
                dateTime.ThisMonth(5),
                dateTime.FirstDayOfMonth(),
                dateTime.LastDayOfMonth(),
                dateTime.FirstDayOfWeek(),
                dateTime.LastDayOfWeek(),
            },
            result => Assert.Equal(kind, result.Kind));
    }

    [Fact]
    public void Week_Boundaries_Respect_The_Configured_First_Day()
    {
        var dateTime = new DateTime(2024, 2, 14, 12, 34, 56, DateTimeKind.Utc);

        Assert.Equal(new DateTime(2024, 2, 11, 0, 0, 0, DateTimeKind.Utc), dateTime.FirstDayOfWeek(weekStartsOn: DayOfWeek.Sunday));
        Assert.Equal(new DateTime(2024, 2, 17, 0, 0, 0, DateTimeKind.Utc), dateTime.LastDayOfWeek(weekStartsOn: DayOfWeek.Sunday));
    }

    [Fact]
    public void Week_Boundaries_Reject_An_Invalid_First_Day()
    {
        var dateTime = new DateTime(2024, 2, 14);

        Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.FirstDayOfWeek(weekStartsOn: (DayOfWeek)7));
    }

    [Theory]
    [MemberData(nameof(DateTimeKinds))]
    public void LastTick_Methods_Return_The_Final_Tick_Of_Their_Period(DateTimeKind kind)
    {
        var dateTime = new DateTime(2024, 2, 14, 12, 34, 56, 789, kind);

        Assert.Equal(new DateTime(2024, 2, 15, 0, 0, 0, kind).AddTicks(-1), dateTime.LastTickOfDay());
        Assert.Equal(new DateTime(2024, 2, 19, 0, 0, 0, kind).AddTicks(-1), dateTime.LastTickOfWeek());
        Assert.Equal(new DateTime(2024, 3, 1, 0, 0, 0, kind).AddTicks(-1), dateTime.LastTickOfMonth());
        Assert.Equal(kind, dateTime.LastTickOfDay().Kind);
        Assert.Equal(kind, dateTime.LastTickOfWeek().Kind);
        Assert.Equal(kind, dateTime.LastTickOfMonth().Kind);
    }

    [Fact]
    public void LastTick_Methods_Support_The_Last_Representable_Day_And_Month()
    {
        Assert.Equal(DateTime.MaxValue, DateTime.MaxValue.LastTickOfDay());
        Assert.Equal(DateTime.MaxValue, DateTime.MaxValue.LastTickOfMonth());
    }

    [Fact]
    public void Nullable_LastTickOfDay_Preserves_Null()
    {
        DateTime? dateTime = null;

        Assert.Null(dateTime.LastTickOfDay());
    }

    [Fact]
    public void ToDateTimeOffset_WithoutOffset_ShouldMatchFrameworkConstructor()
    {
        var dateTime = new DateTime(2024, 2, 14, 12, 34, 56, DateTimeKind.Unspecified);

        Assert.Equal(new DateTimeOffset(dateTime), dateTime.ToDateTimeOffset());
    }

    [Fact]
    public void ToDateTimeOffset_ShouldUseExplicitOffsetForUnspecifiedDateTime()
    {
        var dateTime = new DateTime(2024, 2, 14, 12, 34, 56, DateTimeKind.Unspecified);
        var offset = TimeSpan.FromHours(8);

        var result = dateTime.ToDateTimeOffset(offset);

        Assert.Equal(new DateTimeOffset(dateTime, offset), result);
        Assert.Equal(result.ToUnixTimeSeconds(), dateTime.ToUnixTimeSeconds(offset));
        Assert.Equal(result.ToUnixTimeMilliseconds(), dateTime.ToUnixTimeMilliseconds(offset));
    }

    [Fact]
    public void ToDateTimeOffset_WithOffset_ShouldPreserveFrameworkValidation()
    {
        var utc = new DateTime(2024, 2, 14, 12, 34, 56, DateTimeKind.Utc);

        Assert.Throws<ArgumentException>(() => utc.ToDateTimeOffset(TimeSpan.FromHours(1)));
    }
}
