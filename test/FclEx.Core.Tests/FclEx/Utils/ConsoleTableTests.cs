namespace FclEx.Utils;

public class ConsoleTableTests
{
    private static readonly bool[] BoolValues = [true, false];

    public static readonly TheoryData<bool, bool, bool> TestCases = BoolValues.CrossJoinCube().ToTheoryData();

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Render_Test(bool hasColumn, bool hasRow, bool hasTitle)
    {
        var columns = new[] { "LongLongLongLong", "Sh" };
        var rows = new string[][]
        {
            ["SuccessCount", "1762"],
            ["FailedCount", "22"],
            ["InvalidCount", "85"],
            ["TotalCount", "1869"],
        };
        var table = new ConsoleTable(new()
        {
            Columns = columns,
            RenderColumns = hasColumn,
            Title = hasTitle ? "Title" : null
        });

        if (hasRow)
        {
            foreach (var row in rows)
            {
                table.AddRow(row);
            }
        }

        var sb = new StringBuilder(1024);
        table.Render(sb);

        if (TestHelper.IsRunningUnderReSharper())
        {
            TestContext.Current.TestOutputHelper?.WriteLine(sb.ToString());
        }
    }

    [Fact]
    public void Constructor_Should_Defensively_Copy_Columns()
    {
        object[] columns = ["first", "second"];
        var table = new ConsoleTable(new() { Columns = columns });
        columns[0] = "changed";

        Assert.Equal("first", table.Columns[0]);
        Assert.False(table.Columns is object?[]);
    }

    [Fact]
    public void AddRow_Should_Defensively_Copy_Row()
    {
        var table = new ConsoleTable(new() { Columns = ["first", "second"] });
        object?[] row = ["first", "second"];
        table.AddRow(row);
        row[0] = "changed";

        Assert.Equal("first", table.Rows[0][0]);
        Assert.False(table.Rows[0] is object?[]);
    }
}
