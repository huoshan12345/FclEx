namespace FclEx.Utils;

public class ConsoleTableTests
{
    private readonly ITestOutputHelper _output;

    public ConsoleTableTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly bool[] BoolValues = [true, false];

    public static readonly IEnumerable<object[]> TestCases =
        from x in BoolValues
        from y in BoolValues
        from z in BoolValues
        select new object[] { x, y, z };

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
        _output.WriteLine(sb.ToString());
    }
}